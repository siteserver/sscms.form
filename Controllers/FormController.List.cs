using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Models;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers
{
    public partial class FormController
    {
        [HttpGet, Route("{siteId:int}/{formId:int}")]
        public async Task<ActionResult<ListResult>> List([FromRoute] int siteId, [FromRoute] int formId, [FromQuery] ListRequest request)
        {
            var formInfo = await _formRepository.GetFormInfoAsync(siteId, formId);
            if (formInfo == null) return NotFound();

            var tableName = _formManager.GetTableName(formInfo);
            var relatedIdentities = _formManager.GetRelatedIdentities(formInfo);
            var styles = await _formManager.GetTableStylesAsync(tableName, relatedIdentities);

            var listAttributeNames = ListUtils.GetStringList(formInfo.ListAttributeNames);
            var allAttributeNames = _formRepository.GetAllAttributeNames(styles);
            var pageSize = _formManager.GetPageSize(formInfo);

            var (totalCount, dataInfoList) = await _dataRepository.GetDataAsync(formInfo, formInfo.IsReply, request.Word, request.Page, pageSize);
            var pages = Convert.ToInt32(Math.Ceiling((double)totalCount / pageSize));
            if (pages == 0) pages = 1;
            var page = request.Page;
            if (page > pages) page = pages;


            var logs = new List<IDictionary<string, object>>();
            foreach (var dataInfo in dataInfoList)
            {
                logs.Add(dataInfo.ToDictionary());
            }

            return new ListResult
            {
                Logs = logs,
                Count = totalCount,
                Pages = pages,
                Page = page,
                Styles = styles,
                AllAttributeNames = allAttributeNames,
                ListAttributeNames = listAttributeNames,
                IsReply = formInfo.IsReply
            };
        }

        public class ListRequest
        {
            public int Page { get; set; }
            public string Word { get; set; }
        }

        public class ListResult
        {
            public List<IDictionary<string, object>> Logs { get; set; }
            public int Count { get; set; }
            public int Pages { get; set; }
            public int Page { get; set; }
            public List<TableStyle> Styles { get; set; }
            public List<string> AllAttributeNames { get; set; }
            public List<string> ListAttributeNames { get; set; }
            public bool IsReply { get; set; }
        }
    }
}
