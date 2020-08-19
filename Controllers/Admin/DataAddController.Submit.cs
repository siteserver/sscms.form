using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Enums;
using SSCMS.Form.Core;
using SSCMS.Form.Models;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class DataAddController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<BoolResult>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formManager.GetFormInfoByRequestAsync(request.SiteId, request.ChannelId, request.ContentId, request.FormId);
            if (formInfo == null) return NotFound();

            var dataInfo = request.DataId > 0
                ? await _dataRepository.GetDataInfoAsync(request.DataId)
                : new DataInfo
                {
                    FormId = formInfo.Id
                };

            var tableName = _formManager.GetTableName(formInfo);
            var relatedIdentities = _formManager.GetRelatedIdentities(formInfo);
            var styles = await _formManager.GetTableStylesAsync(tableName, relatedIdentities);

            foreach (var style in styles)
            {

                var inputType = style.InputType;
                if (inputType == InputType.CheckBox ||
                    style.InputType == InputType.SelectMultiple)
                {
                    var list = request.Get<List<object>>(style.AttributeName);
                    dataInfo.Set(style.AttributeName, ListUtils.ToString(list));
                }
                else
                {
                    var value = request.Get(style.AttributeName, string.Empty);
                    dataInfo.Set(style.AttributeName, value);
                }
            }

            if (request.DataId == 0)
            {
                dataInfo.Id = await _dataRepository.InsertAsync(formInfo, dataInfo);
                _formManager.SendNotify(formInfo, styles, dataInfo);
            }
            else
            {
                await _dataRepository.UpdateAsync(dataInfo);
            }

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
