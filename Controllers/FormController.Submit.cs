﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Models;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers
{
    public partial class FormController
    {
        [HttpPost, Route("{siteId:int}/{formId:int}")]
        public async Task<ActionResult<DataInfo>> Submit([FromRoute] int siteId, [FromRoute] int formId, [FromBody] DataInfo request)
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

            request.FormId = formId;

            request.Id = await _dataRepository.InsertAsync(formInfo, request);
            await _formManager.SendNotifyAsync(formInfo, styles, request);

            return request;
        }
    }
}
