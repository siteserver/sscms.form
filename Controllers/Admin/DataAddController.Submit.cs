using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Core;
using SSCMS.Form.Models;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class DataAddController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<BoolResult>> Submit([FromQuery] int siteId, [FromBody] DataInfo request)
        {
            var formInfo = await _formRepository.GetFormInfoAsync(siteId, request.FormId);
            if (!await _authManager.HasSitePermissionsAsync(siteId, FormManager.PermissionsForms))
                return Unauthorized();

            if (formInfo == null) return NotFound();

            //var dataInfo = request.DataId > 0
            //    ? await _dataRepository.GetDataInfoAsync(request.DataId)
            //    : new DataInfo
            //    {
            //        FormId = formInfo.Id
            //    };

            var styles = await _formManager.GetTableStylesAsync(formInfo.Id);

            //foreach (var style in styles)
            //{
            //    var inputType = style.InputType;
            //    if (inputType == InputType.CheckBox ||
            //        style.InputType == InputType.SelectMultiple)
            //    {
            //        var list = request.Get<List<string>>(style.AttributeName);
            //        dataInfo.Set(style.AttributeName, ListUtils.ToString(list));
            //    }
            //    else
            //    {
            //        var value = request.Get(style.AttributeName, string.Empty);
            //        dataInfo.Set(style.AttributeName, value);
            //    }
            //}

            if (request.Id == 0)
            {
                request.SiteId = siteId;
                request.ChannelId = 0;
                request.ContentId = 0;
                request.Id = await _dataRepository.InsertAsync(formInfo, request);
                await _formManager.SendNotifyAsync(formInfo, styles, request);
            }
            else
            {
                await _dataRepository.UpdateAsync(request);
            }

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
