using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Core;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class DataAddController
    {
        [HttpDelete, Route(RouteActionsUpload)]
        public async Task<ActionResult<BoolResult>> DeleteFile([FromBody] DeleteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var filePath = PathUtils.Combine(_pathManager.ContentRootPath, request.FileUrl);
            FileUtils.DeleteFileIfExists(filePath);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
