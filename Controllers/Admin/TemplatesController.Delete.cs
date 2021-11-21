using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class TemplatesController
    {
        [HttpPost, Route(RouteDelete)]
        public async Task<ActionResult<ListResult>> Delete([FromBody] DeleteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsTemplates))
                return Unauthorized();

            _formManager.DeleteTemplate(request.Name);

            return new ListResult
            {
                TemplateInfoList = _formManager.GetTemplateInfoList(request.Type)
            };
        }
    }
}
