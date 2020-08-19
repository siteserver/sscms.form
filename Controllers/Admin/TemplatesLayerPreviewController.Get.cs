using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Core;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class TemplatesLayerPreviewController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] GetRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsTemplates))
                return Unauthorized();

            var formInfoList = await _formRepository.GetFormInfoListAsync(request.SiteId, 0);

            var templateInfoList = _formManager.GetTemplateInfoList(request.Type);
            var templateInfo =
                templateInfoList.FirstOrDefault(x => StringUtils.EqualsIgnoreCase(request.Name, x.Name));

            return new GetResult
            {
                TemplateInfo = templateInfo,
                FormInfoList = formInfoList
            };
        }
    }
}
