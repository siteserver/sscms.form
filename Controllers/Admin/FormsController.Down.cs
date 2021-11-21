using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class FormsController
    {
        [HttpPost, Route(RouteDown)]
        public async Task<ActionResult<TaxisResult>> Down([FromBody] TaxisRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            await _formRepository.UpdateTaxisToDownAsync(request.SiteId, request.FormId);

            var formInfoList = await _formRepository.GetFormInfoListAsync(request.SiteId);

            return new TaxisResult
            {
                FormInfoList = formInfoList
            };
        }
    }
}
