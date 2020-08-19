using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Core;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class DataController
    {
        [HttpPost, Route(ActionsColumns)]
        public async Task<ActionResult<BoolResult>> Columns([FromBody] ColumnsRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formManager.GetFormInfoByRequestAsync(request.SiteId, request.ChannelId, request.ContentId, request.FormId);
            if (formInfo == null) return NotFound();

            formInfo.ListAttributeNames = ListUtils.ToString(request.AttributeNames);
            await _formRepository.UpdateAsync(formInfo);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
