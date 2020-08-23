using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Core;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class FormsController
    {
        [HttpPost, Route(RouteExport)]
        public async Task<ActionResult<StringResult>> Export([FromBody] FormRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formManager.GetFormInfoByRequestAsync(request.SiteId, request.ChannelId, request.ContentId, request.FormId);
            if (formInfo == null) return NotFound();

            var fileName = $"{formInfo.Title}.zip";
            var directoryPath = _pathManager.GetTemporaryFilesPath("form");
            DirectoryUtils.DeleteDirectoryIfExists(directoryPath);

            await _formManager.ExportFormAsync(formInfo.SiteId, directoryPath, formInfo.Id);

            _pathManager.CreateZip(_pathManager.GetTemporaryFilesPath(fileName), directoryPath);

            var url = _pathManager.GetTemporaryFilesUrl($"{fileName}");

            return new StringResult
            {
                Value = url
            };
        }
    }
}
