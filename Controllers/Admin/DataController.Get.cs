using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Enums;
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

            var formInfo = await _formManager.GetFormInfoByRequestAsync(request.SiteId, request.ChannelId, request.ContentId, request.FormId);
            if (formInfo == null) return NotFound();

            var tableName = _formManager.GetTableName(request);
            var relatedIdentities = _formManager.GetRelatedIdentities(request);
            var styles = await _formManager.GetTableStylesAsync(tableName, relatedIdentities);

            var listAttributeNames = ListUtils.GetStringList(formInfo.ListAttributeNames);
            var allAttributeNames = _formRepository.GetAllAttributeNames(styles);
            var pageSize = _formManager.GetPageSize(formInfo);

            var (total, dataInfoList) = await _dataRepository.GetDataAsync(formInfo, false, null, request.Page, pageSize);
            var items = dataInfoList.Select(dataInfo => _dataRepository.GetDict(styles, dataInfo)).ToList();

            var columns = new List<ContentColumn>
            {
                new ContentColumn
                {
                    AttributeName = nameof(DataInfo.Id),
                    DisplayName = "Id",
                    IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, nameof(DataInfo.Id))
                },
                new ContentColumn
                {
                    AttributeName = nameof(DataInfo.Guid),
                    DisplayName = "编号",
                    IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, nameof(DataInfo.Guid))
                }
            };

            foreach (var style in styles)
            {
                if (string.IsNullOrEmpty(style.DisplayName) || style.InputType == InputType.TextEditor) continue;

                var column = new ContentColumn
                {
                    AttributeName = style.AttributeName,
                    DisplayName = style.DisplayName,
                    InputType = style.InputType,
                    IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, style.AttributeName)
                };

                columns.Add(column);
            }

            columns.AddRange(new List<ContentColumn>
            {
                
                new ContentColumn
                {
                    AttributeName = nameof(DataInfo.CreatedDate),
                    DisplayName = "添加时间",
                    IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, nameof(DataInfo.CreatedDate))
                },
                new ContentColumn
                {
                    AttributeName = nameof(DataInfo.LastModifiedDate),
                    DisplayName = "更新时间",
                    IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, nameof(DataInfo.LastModifiedDate))
                }
            });

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
