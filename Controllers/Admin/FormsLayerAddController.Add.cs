using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Core;
using SSCMS.Form.Models;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class FormsLayerAddController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<BoolResult>> Add([FromBody] AddRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = new FormInfo
            {
                SiteId = request.SiteId,
                Title = request.Title,
                Description = request.Description,
                ListAttributeNames = FormManager.DefaultListAttributeNames
            };

            await _formRepository.InsertAsync(formInfo);

            await _formManager.CreateDefaultStylesAsync(formInfo);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
