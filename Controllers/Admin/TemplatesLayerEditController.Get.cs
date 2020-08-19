using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Core;
using SSCMS.Form.Models;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class TemplatesLayerEditController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] GetRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsTemplates))
                return Unauthorized();

            var templateInfo = _formManager.GetTemplateInfo(request.Name);

            if (!string.IsNullOrEmpty(templateInfo.Publisher))
            {
                templateInfo = new TemplateInfo();
            }

            return new GetResult
            {
                TemplateInfo = templateInfo
            };
        }
    }
}
