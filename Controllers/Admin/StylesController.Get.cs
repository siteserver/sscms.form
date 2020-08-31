using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Enums;
using SSCMS.Form.Core;
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

            var tableName = _formManager.GetTableName(request);
            var relatedIdentities = _formManager.GetRelatedIdentities(request);
            var styles = await _formManager.GetTableStylesAsync(tableName, relatedIdentities);

            var inputTypes = ListUtils.GetSelects<InputType>();

            return new GetResult
            {
                InputTypes = inputTypes,
                TableName = tableName,
                RelatedIdentities = ListUtils.ToString(relatedIdentities),
                Styles = styles,
            };
        }
    }
}
