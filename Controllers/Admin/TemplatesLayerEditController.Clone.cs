using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Extensions;
using SSCMS.Form.Core;
using SSCMS.Form.Models;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class TemplatesLayerEditController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<BoolResult>> Clone([FromBody] CloneRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsTemplates))
                return Unauthorized();

            var templateInfoList = _formManager.GetTemplateInfoList(request.Type);
            var originalTemplateInfo = templateInfoList.First(x => StringUtils.EqualsIgnoreCase(request.OriginalName, x.Name));

            if (templateInfoList.Any(x => StringUtils.EqualsIgnoreCase(request.Name, x.Name)))
            {
                return this.Error($"标识为 {request.Name} 的模板已存在，请更换模板标识！");
            }

            var templateInfo = new TemplateInfo
            {
                Type = request.Type,
                Name = request.Name,
                Main = originalTemplateInfo.Main,
                Publisher = string.Empty,
                Description = request.Description,
                Icon = originalTemplateInfo.Icon
            };
            templateInfoList.Add(templateInfo);

            _formManager.Clone(request.OriginalName, templateInfo, request.TemplateHtml);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
