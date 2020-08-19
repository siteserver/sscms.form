using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class FormsLayerAddController
    {
        [HttpPut, Route(Route)]
        public async Task<ActionResult<BoolResult>> Edit([FromBody] EditRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formRepository.GetFormInfoAsync(request.SiteId, request.FormId);
            formInfo.Title = request.Title;
            formInfo.Description = request.Description;

            await _formRepository.UpdateAsync(formInfo);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
