using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class TemplateHtmlController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<BoolResult>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsTemplates))
                return Unauthorized();

            var templateInfo = _formManager.GetTemplateInfo(request.Name);

            _formManager.SetTemplateHtml(templateInfo, request.TemplateHtml);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
