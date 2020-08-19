using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Extensions;
using SSCMS.Form.Models;

namespace SSCMS.Form.Controllers
{
    public partial class FormController
    {
        [HttpPost, Route("{siteId:int}/{formId:int}")]
        public async Task<ActionResult<DataInfo>> Submit([FromRoute] int siteId, [FromRoute] int formId)
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

            var dataInfo = new DataInfo
            {
                FormId = formInfo.Id
            };

            var tableName = _formManager.GetTableName(formInfo);
            var relatedIdentities = _formManager.GetRelatedIdentities(formInfo);
            var styles = await _formManager.GetTableStylesAsync(tableName, relatedIdentities);

            foreach (var style in styles)
            {
                //TODO
                //var value = request.GetPostString(style.Title);
                //dataInfo.Set(style.Title, value);
                //if (IFieldManager.IsExtra(style))
                //{
                //    foreach (var item in style.Items)
                //    {
                //        var extrasId = IFieldManager.GetExtrasId(style.Id, item.Id);
                //        var extras = request.GetPostString(extrasId);
                //        if (!string.IsNullOrEmpty(extras))
                //        {
                //            dataInfo.Set(extrasId, extras);
                //        }
                //    }
                //}
            }

            dataInfo.Id = await _dataRepository.InsertAsync(formInfo, dataInfo);
            _formManager.SendNotify(formInfo, styles, dataInfo);

            return dataInfo;
        }
    }
}
