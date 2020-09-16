using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Core;
using SSCMS.Form.Models;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class TemplatesLayerEditController
    {
        [HttpPut, Route(Route)]
        public async Task<ActionResult<BoolResult>> Edit([FromBody] EditRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsTemplates))
                return Unauthorized();

            if (StringUtils.EqualsIgnoreCase(request.OriginalName, request.Name))
            {
                var templateInfoList = _formManager.GetTemplateInfoList(request.Type);
                var originalTemplateInfo = templateInfoList.First(x => StringUtils.EqualsIgnoreCase(request.OriginalName, x.Name));

                originalTemplateInfo.Name = request.Name;
                originalTemplateInfo.Description = request.Description;
                _formManager.Edit(originalTemplateInfo);
            }
            else
            {
                var templateInfoList = _formManager.GetTemplateInfoList(request.Type);
                var originalTemplateInfo = templateInfoList.First(x => StringUtils.EqualsIgnoreCase(request.OriginalName, x.Name));

                if (templateInfoList.Any(x => StringUtils.EqualsIgnoreCase(request.Name, x.Name)))
                {
                    return this.Error($"标识为 {request.Name} 的模板已存在，请更换模板标识！");
                }

                var templateInfo = new TemplateInfo
                {
                    Name = request.Name,
                    Type = request.Type,
                    Main = originalTemplateInfo.Main,
                    Publisher = string.Empty,
                    Description = request.Description,
                    Icon = originalTemplateInfo.Icon
                };
                templateInfoList.Add(templateInfo);

                _formManager.Clone(request.OriginalName, templateInfo);

                _formManager.DeleteTemplate(request.OriginalName);
            }

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
