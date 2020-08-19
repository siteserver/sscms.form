using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Core;
using SSCMS.Form.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class DataController
    {
        [HttpPost, Route(ActionsExport)]
        public async Task<ActionResult<StringResult>> Export([FromBody] FormRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formManager.GetFormInfoByRequestAsync(request.SiteId, request.ChannelId, request.ContentId, request.FormId);
            if (formInfo == null) return NotFound();

            var tableName = _formManager.GetTableName(request);
            var relatedIdentities = _formManager.GetRelatedIdentities(request);
            var styles = await _formManager.GetTableStylesAsync(tableName, relatedIdentities);

            var logs = await _dataRepository.GetAllDataInfoListAsync(formInfo);

            var head = new List<string> { "编号" };
            foreach (var style in styles)
            {
                head.Add(style.DisplayName);
            }
            head.Add("添加时间");

            var rows = new List<List<string>>();

            foreach (var log in logs)
            {
                var row = new List<string>
                {
                    log.Guid
                };
                foreach (var style in styles)
                {
                    row.Add(_dataRepository.GetValue(style, log));
                }

                if (log.CreatedDate.HasValue)
                {
                    row.Add(log.CreatedDate.Value.ToString("yyyy-MM-dd HH:mm"));
                }

                rows.Add(row);
            }

            var fileName = $"{formInfo.Title}.csv";

            CsvUtils.Export(_pathManager.GetTemporaryFilesPath(fileName), head, rows);
            var downloadUrl = _pathManager.GetTemporaryFilesUrl(fileName);

            return new StringResult
            {
                Value = downloadUrl
            };
        }
    }
}
