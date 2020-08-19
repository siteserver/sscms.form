using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class FormsController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] SiteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfoList = await _formRepository.GetFormInfoListAsync(request.SiteId, 0);

            return new GetResult
            {
                FormInfoList = formInfoList
            };
        }
    }
}
