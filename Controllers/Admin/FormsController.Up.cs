using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class FormsController
    {
        [HttpPost, Route(RouteUp)]
        public async Task<ActionResult<TaxisResult>> Up([FromBody] TaxisRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            await _formRepository.UpdateTaxisToUpAsync(request.SiteId, request.FormId);

            var formInfoList = await _formRepository.GetFormInfoListAsync(request.SiteId);

            return new TaxisResult
            {
                FormInfoList = formInfoList
            };
        }
    }
}
