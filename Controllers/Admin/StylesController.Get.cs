using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Enums;
using SSCMS.Form.Core;
using SSCMS.Form.Utils;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class StylesController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] FormRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                Types.SitePermissions.SettingsStyleSite))
            {
                return Unauthorized();
            }

            var formInfo = await _formManager.GetFormInfoByRequestAsync(request.SiteId, request.ChannelId, request.ContentId, request.FormId);
            if (formInfo == null) return NotFound();

            var styles = await _formManager.GetTableStylesAsync(formInfo.Id);

            var inputTypes = ListUtils.GetSelects<InputType>();

            return new GetResult
            {
                InputTypes = inputTypes,
                TableName = FormUtils.TableNameData,
                RelatedIdentities = ListUtils.ToString(_formManager.GetRelatedIdentities(formInfo.Id)),
                Styles = styles,
            };
        }
    }
}
