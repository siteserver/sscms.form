using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Extensions;
using SSCMS.Models;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers
{
    public partial class FormController
    {
        [HttpPost, Route("{siteId:int}/{formId:int}/actions/get")]
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

            var tableName = _formManager.GetTableName(formInfo);
            var relatedIdentities = _formManager.GetRelatedIdentities(formInfo);
            var styles = await _formManager.GetTableStylesAsync(tableName, relatedIdentities);

            var uploadToken = StringUtils.GetShortGuid(false);

            var cacheKey = GetUploadTokenCacheKey(formId);
            var cacheList = _cacheManager.Get(cacheKey) ?? new List<string>();
            cacheList.Add(uploadToken);
            _cacheManager.AddOrUpdate(cacheKey, cacheList);

            return new GetFormResult
            {
                Styles = styles,
                Title = formInfo.Title,
                Description = formInfo.Description,
                IsCaptcha = formInfo.IsCaptcha,
                UploadToken = uploadToken
            };
        }

        public class GetFormResult
        {
            public List<TableStyle> Styles { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public bool IsCaptcha { get; set; }
            public string UploadToken { get; set; }
        }
    }
}
