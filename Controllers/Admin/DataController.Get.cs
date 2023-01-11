using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Form.Core;
using SSCMS.Form.Models;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class DataController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] GetRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formRepository.GetFormInfoAsync(request.SiteId, request.FormId);
            if (formInfo == null) return NotFound();

            var styles = await _formManager.GetTableStylesAsync(formInfo.Id);

            var listAttributeNames = ListUtils.GetStringList(formInfo.ListAttributeNames);
            var allAttributeNames = _formRepository.GetAllAttributeNames(styles);
            var pageSize = _formManager.GetPageSize(formInfo);

            var (total, dataInfoList) = await _dataRepository.GetListAsync(formInfo, false, request.StartDate, request.EndDate, request.Keyword, request.Page, pageSize);
            var items = new List<DataInfo>();
            foreach (var dataInfo in dataInfoList)
            {
                var item = dataInfo.Clone<DataInfo>();
                if (dataInfo.ChannelId > 0 && listAttributeNames.Contains(nameof(DataInfo.ChannelId)))
                {
                    var channelName = _channelRepository.GetChannelNameNavigationAsync(dataInfo.SiteId, dataInfo.ChannelId);
                    item.Set("channelPage", channelName);
                }
                if (dataInfo.ContentId > 0 && listAttributeNames.Contains(nameof(DataInfo.ContentId)))
                {
                    var content = await _contentRepository.GetAsync(dataInfo.SiteId, dataInfo.ChannelId, dataInfo.ContentId);
                    var title = content != null ? content.Title : string.Empty;
                    item.Set("contentPage", title);
                }
                items.Add(item);
            }

            var columns = _formManager.GetColumns(listAttributeNames, styles, formInfo.IsReply);

            var isSmsEnabled = await _smsManager.IsSmsEnabledAsync();
            if (isSmsEnabled && formInfo.IsSms)
            {
                allAttributeNames.Add("SmsMobile");
                columns.Add(new ContentColumn
                {
                    AttributeName = "SmsMobile",
                    DisplayName = "短信验证手机号码",
                    IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, "SmsMobile")
                });
            }

            return new GetResult
            {
                Items = items,
                Total = total,
                PageSize = pageSize,
                Styles = styles,
                AllAttributeNames = allAttributeNames,
                ListAttributeNames = listAttributeNames,
                IsReply = formInfo.IsReply,
                Columns = columns
            };
        }
    }
}
