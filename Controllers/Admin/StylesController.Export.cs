using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class StylesController
    {
        [HttpPost, Route(RouteExport)]
        public async Task<ActionResult<StringResult>> Export([FromBody] FormRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                FormManager.PermissionsForms))
            {
                return Unauthorized();
            }

            var tableName = _formManager.GetTableName(request);
            var relatedIdentities = _formManager.GetRelatedIdentities(request);

            var fileName = await _pathManager.ExportStylesAsync(request.SiteId, tableName, relatedIdentities);

            var filePath = _pathManager.GetTemporaryFilesPath(fileName);
            var downloadUrl = _pathManager.GetRootUrlByPath(filePath);

            return new StringResult
            {
                Value = downloadUrl
            };
        }
    }
}
