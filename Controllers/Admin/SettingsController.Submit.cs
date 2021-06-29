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

            var formInfo = await _formRepository.GetFormInfoAsync(request.SiteId, request.FormId);
            if (formInfo == null) return NotFound();

            formInfo.IsClosed = request.IsClosed;
            formInfo.Title = request.Title;
            formInfo.Description = request.Description;
            formInfo.IsReply = request.IsReply;
            formInfo.PageSize = request.PageSize;
            formInfo.IsTimeout = request.IsTimeout;
            formInfo.TimeToStart = request.TimeToStart;
            formInfo.TimeToEnd = request.TimeToEnd;
            formInfo.IsCaptcha = request.IsCaptcha;
            formInfo.IsAdministratorSmsNotify = request.IsAdministratorSmsNotify;
            formInfo.AdministratorSmsNotifyTplId = request.AdministratorSmsNotifyTplId;
            formInfo.AdministratorSmsNotifyKeys = request.AdministratorSmsNotifyKeys;
            formInfo.AdministratorSmsNotifyMobile = request.AdministratorSmsNotifyMobile;
            formInfo.IsAdministratorMailNotify = request.IsAdministratorMailNotify;
            formInfo.AdministratorMailTitle = request.AdministratorMailTitle;
            formInfo.AdministratorMailNotifyAddress = request.AdministratorMailNotifyAddress;
            formInfo.IsUserSmsNotify = request.IsUserSmsNotify;
            formInfo.UserSmsNotifyTplId = request.UserSmsNotifyTplId;
            formInfo.UserSmsNotifyKeys = request.UserSmsNotifyKeys;
            formInfo.UserSmsNotifyMobileName = request.UserSmsNotifyMobileName;

            await _formRepository.UpdateAsync(formInfo);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
