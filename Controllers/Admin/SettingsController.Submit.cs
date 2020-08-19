using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class SettingsController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<BoolResult>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formManager.GetFormInfoByRequestAsync(request.SiteId, request.ChannelId, request.ContentId, request.FormId);
            if (formInfo == null) return NotFound();

            formInfo.IsClosed = request.Form.IsClosed;
            formInfo.Title = request.Form.Title;
            formInfo.Description = request.Form.Description;
            formInfo.IsReply = request.Form.IsReply;
            formInfo.PageSize = request.Form.PageSize;
            formInfo.IsTimeout = request.Form.IsTimeout;
            formInfo.TimeToStart = request.Form.TimeToStart;
            formInfo.TimeToEnd = request.Form.TimeToEnd;
            formInfo.IsCaptcha = request.Form.IsCaptcha;
            formInfo.IsAdministratorSmsNotify = request.Form.IsAdministratorSmsNotify;
            formInfo.AdministratorSmsNotifyTplId = request.Form.AdministratorSmsNotifyTplId;
            formInfo.AdministratorSmsNotifyKeys = request.Form.AdministratorSmsNotifyKeys;
            formInfo.AdministratorSmsNotifyMobile = request.Form.AdministratorSmsNotifyMobile;
            formInfo.IsAdministratorMailNotify = request.Form.IsAdministratorMailNotify;
            formInfo.AdministratorMailNotifyAddress = request.Form.AdministratorMailNotifyAddress;
            formInfo.IsUserSmsNotify = request.Form.IsUserSmsNotify;
            formInfo.UserSmsNotifyTplId = request.Form.UserSmsNotifyTplId;
            formInfo.UserSmsNotifyKeys = request.Form.UserSmsNotifyKeys;
            formInfo.UserSmsNotifyMobileName = request.Form.UserSmsNotifyMobileName;

            await _formRepository.UpdateAsync(formInfo);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
