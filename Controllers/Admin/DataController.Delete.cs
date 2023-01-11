using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class DataController
    {
        [HttpPost, Route(RouteDelete)]
        public async Task<ActionResult<BoolResult>> Delete([FromBody] DeleteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formRepository.GetFormInfoAsync(request.SiteId, request.FormId);
            if (formInfo == null) return NotFound();

            foreach (var dataId in request.DataIds)
            {
                var dataInfo = await _dataRepository.GetDataInfoAsync(dataId);
                await _dataRepository.DeleteAsync(formInfo, dataInfo);
            }

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
