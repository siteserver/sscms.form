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

            var formInfo = await _formRepository.GetFormInfoAsync(request.SiteId, request.FormId);
            if (formInfo == null) return NotFound();

            var styles = await _formManager.GetTableStylesAsync(formInfo.Id);

            var attributeNames = _formRepository.GetAllAttributeNames(styles);
            attributeNames.Remove(nameof(DataInfo.IsReplied));
            attributeNames.Remove(nameof(DataInfo.ReplyDate));
            attributeNames.Remove(nameof(DataInfo.ReplyContent));
            var administratorSmsNotifyKeys = ListUtils.GetStringList(formInfo.AdministratorSmsNotifyKeys);
            var userSmsNotifyKeys = ListUtils.GetStringList(formInfo.UserSmsNotifyKeys);

            var isSmsEnabled = await _smsManager.IsSmsEnabledAsync();
            var isMailEnabled = await _mailManager.IsMailEnabledAsync();

            if (string.IsNullOrEmpty(formInfo.SuccessMessage))
            {
                formInfo.SuccessMessage = "表单提交成功！";
            }

            return new GetResult
            {
                Form = formInfo,
                Styles = styles,
                AttributeNames = attributeNames,
                AdministratorSmsNotifyKeys = administratorSmsNotifyKeys,
                UserSmsNotifyKeys = userSmsNotifyKeys,
                IsSmsEnabled = isSmsEnabled,
                IsMailEnabled = isMailEnabled
            };
        }
    }
}
