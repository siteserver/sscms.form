using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class FormsController
    {
        [HttpDelete, Route(Route)]
        public async Task<ActionResult<DeleteResult>> Delete([FromBody] DeleteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            await _formRepository.DeleteAsync(request.SiteId, request.FormId);

            var formInfoList = await _formRepository.GetFormInfoListAsync(request.SiteId, 0);

            return new DeleteResult
            {
                FormInfoList = formInfoList
            };
        }
    }
}
