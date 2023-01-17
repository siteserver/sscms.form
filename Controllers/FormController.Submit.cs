using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Models;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers
{
    public partial class FormController
    {
        [HttpPost, Route("")]
        public async Task<ActionResult<DataInfo>> Submit([FromBody] DataInfo request)
        {
            var formInfo = await _formRepository.GetFormInfoAsync(request.SiteId, request.FormId);
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
            if (isSmsEnabled && formInfo.IsSms)
            {
                var codeCacheKey = GetSmsCodeCacheKey(formInfo.Id, request.Get<string>("SmsMobile"));
                var code = _cacheManager.Get<int>(codeCacheKey);
                if (code == 0 || TranslateUtils.ToInt(request.Get<string>("SmsCode")) != code)
                {
                    return this.Error("输入的验证码有误或验证码已超时");
                }
            }

            var styles = await _formManager.GetTableStylesAsync(formInfo.Id);

            request.Id = await _dataRepository.InsertAsync(formInfo, request);
            await _formManager.SendNotifyAsync(formInfo, styles, request);

            return request;
        }
    }
}
