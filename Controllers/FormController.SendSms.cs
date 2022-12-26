using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Enums;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers
{
    public partial class FormController
    {
        [HttpPost, Route("{siteId:int}/{formId:int}/actions/sendSms")]
        public async Task<ActionResult<BoolResult>> SendSms([FromRoute] int siteId, [FromRoute] int formId, [FromBody] SendSmsRequest request)
        {
            var formInfo = await _formRepository.GetFormInfoAsync(siteId, formId);
            if (formInfo == null) return NotFound();
            if (formInfo.IsClosed)
            {
                return this.Error("对不起，表单已被禁用");
            }

            if (formInfo.IsTimeout && (formInfo.TimeToStart > DateTime.Now || formInfo.TimeToEnd < DateTime.Now))
            {
                return this.Error("对不起，表单只允许在规定的时间内提交");
            }

            var isSmsEnabled = await _smsManager.IsSmsEnabledAsync();
            if (!isSmsEnabled || !formInfo.IsSms)
            {
                return this.Error("对不起，表单短信验证功能已被禁用");
            }

            var code = StringUtils.GetRandomInt(100000, 999999);
            var (success, errorMessage) =
                await _smsManager.SendSmsAsync(request.Mobile, SmsCodeType.Authentication, code);
            if (!success)
            {
                return this.Error(errorMessage);
            }

            var cacheKey = GetSmsCodeCacheKey(formId, request.Mobile);
            _cacheManager.AddOrUpdateAbsolute(cacheKey, code, 10);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
