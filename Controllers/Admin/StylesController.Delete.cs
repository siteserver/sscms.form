using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class StylesController
    {
        [HttpPost, Route(RouteDelete)]
        public async Task<ActionResult<DeleteResult>> Delete([FromBody] DeleteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formRepository.GetFormInfoAsync(request.SiteId, request.FormId);
            if (formInfo == null) return NotFound();

            await _formManager.DeleteTableStyleAsync(formInfo.Id, request.AttributeName);

            var styles = await _formManager.GetTableStylesAsync(formInfo.Id);

            return new DeleteResult
            {
                Styles = styles
            };
        }
    }
}
