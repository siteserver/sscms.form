using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Form.Core;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class StylesController
    {
        [HttpDelete, Route(Route)]
        public async Task<ActionResult<DeleteResult>> Delete([FromBody] DeleteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId,
                FormManager.PermissionsForms))
            {
                return Unauthorized();
            }

            var tableName = _formManager.GetTableName(request);
            var relatedIdentities = _formManager.GetRelatedIdentities(request);

            await _formManager.DeleteTableStyleAsync(tableName, relatedIdentities, request.AttributeName);

            var styles = await _formManager.GetTableStylesAsync(tableName, relatedIdentities);
            foreach (var style in styles)
            {
                style.Rules = TranslateUtils.JsonDeserialize<List<InputStyleRule>>(style.RuleValues);
            }

            return new DeleteResult
            {
                Styles = styles
            };
        }
    }
}
