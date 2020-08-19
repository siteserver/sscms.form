using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class TemplatesController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<ListResult>> Get([FromQuery] ListRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsTemplates))
                return Unauthorized();

            return new ListResult
            {
                TemplateInfoList = _formManager.GetTemplateInfoList(request.Type)
            };
        }
    }
}
