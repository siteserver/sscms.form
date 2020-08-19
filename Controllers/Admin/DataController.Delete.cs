using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class DataController
    {
        [HttpDelete, Route(Route)]
        public async Task<ActionResult<DeleteResult>> Delete([FromBody] DeleteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formManager.GetFormInfoByRequestAsync(request.SiteId, request.ChannelId, request.ContentId, request.FormId);
            if (formInfo == null) return NotFound();

            var page = request.Page;

            var dataInfo = await _dataRepository.GetDataInfoAsync(request.DataId);
            if (dataInfo == null) return NotFound();

            await _dataRepository.DeleteAsync(formInfo, dataInfo);

            var pageSize = _formManager.GetPageSize(formInfo);

            var (total, dataInfoList) = await _dataRepository.GetDataAsync(formInfo, false, null, page, pageSize);
            var pages = Convert.ToInt32(Math.Ceiling((double)total / pageSize));
            if (pages == 0) pages = 1;
            if (page > pages) page = pages;

            var logs = new List<IDictionary<string, object>>();
            foreach (var info in dataInfoList)
            {
                logs.Add(info.ToDictionary());
            }

            return new DeleteResult
            {
                Logs = logs,
                Count = total,
                Pages = pages,
                Page = page
            };
        }
    }
}
