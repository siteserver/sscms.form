using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Core;
using SSCMS.Form.Models;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class SettingsController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] FormRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formManager.GetFormInfoByRequestAsync(request.SiteId, request.ChannelId, request.ContentId, request.FormId);
            if (formInfo == null) return NotFound();

            var tableName = _formManager.GetTableName(request);
            var relatedIdentities = _formManager.GetRelatedIdentities(request);
            var styles = await _formManager.GetTableStylesAsync(tableName, relatedIdentities);

            var attributeNames = _formRepository.GetAllAttributeNames(styles);
            attributeNames.Remove(nameof(DataInfo.IsReplied));
            attributeNames.Remove(nameof(DataInfo.ReplyDate));
            attributeNames.Remove(nameof(DataInfo.ReplyContent));
            var administratorSmsNotifyKeys = ListUtils.GetStringList(formInfo.AdministratorSmsNotifyKeys);
            var userSmsNotifyKeys = ListUtils.GetStringList(formInfo.UserSmsNotifyKeys);

            return new GetResult
            {
                Form = formInfo,
                Styles = styles,
                AttributeNames = attributeNames,
                AdministratorSmsNotifyKeys = administratorSmsNotifyKeys,
                UserSmsNotifyKeys = userSmsNotifyKeys
            };
        }
    }
}
