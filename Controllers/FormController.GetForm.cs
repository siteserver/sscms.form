using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Extensions;

namespace SSCMS.Form.Controllers
{
    public partial class FormController
    {
        [HttpPost, Route("{siteId:int}/{formId:int}/actions/getForm")]
        public async Task<ActionResult<GetFormResult>> GetForm([FromRoute] int siteId, [FromRoute] int formId)
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

            var styles = await _formManager.GetTableStylesAsync(formInfo.Id);

            //var uploadToken = StringUtils.GetShortGuid(false);
            //var cacheKey = GetUploadTokenCacheKey(formId);
            //var cacheList = _cacheManager.Get(cacheKey) ?? new List<string>();
            //cacheList.Add(uploadToken);
            //_cacheManager.AddOrUpdate(cacheKey, cacheList);

            var dataInfo = await _formManager.GetDataInfoAsync(0, formInfo.Id, styles);

            return new GetFormResult
            {
                Styles = styles,
                Title = formInfo.Title,
                Description = formInfo.Description,
                IsCaptcha = formInfo.IsCaptcha,
                DataInfo = dataInfo
            };
        }
    }
}
