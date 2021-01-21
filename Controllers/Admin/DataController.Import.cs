using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Dto;
using SSCMS.Form.Core;
using SSCMS.Form.Models;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class DataController
    {
        [RequestSizeLimit(long.MaxValue)]
        [HttpPost, Route(RouteImport)]
        public async Task<ActionResult<BoolResult>> Import([FromQuery] ImportRequest request, [FromForm] IFormFile file)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formRepository.GetFormInfoAsync(request.SiteId, request.FormId);
            if (formInfo == null) return NotFound();

            if (file == null)
            {
                return this.Error(Constants.ErrorUpload);
            }

            var fileName = PathUtils.GetFileName(file.FileName);

            var sExt = PathUtils.GetExtension(fileName);
            if (!StringUtils.EqualsIgnoreCase(sExt, ".xlsx"))
            {
                return this.Error("导入文件为Excel格式，请选择有效的文件上传");
            }

            var filePath = _pathManager.GetTemporaryFilesPath(fileName);
            await _pathManager.UploadAsync(file, filePath);

            var sheet = ExcelUtils.GetDataTable(filePath);
            if (sheet != null)
            {
                var dataInfos = new List<DataInfo>();
                var styles = await _formManager.GetTableStylesAsync(formInfo.Id);
                var columns = new List<string>();

                for (var i = 1; i < sheet.Rows.Count; i++) //行
                {
                    var row = sheet.Rows[i];

                    if (i == 1)
                    {
                        for (var j = 0; j < sheet.Columns.Count; j++)
                        {
                            var value = row[j].ToString().Trim();
                            columns.Add(value);
                        }
                        continue;
                    }

                    var dict = new Dictionary<string, object>();

                    for (var j = 0; j < columns.Count; j++)
                    {
                        var columnName = columns[j];
                        var value = row[j].ToString().Trim();

                        if (StringUtils.EqualsIgnoreCase(columnName, "是否回复"))
                        {
                            dict[nameof(DataInfo.IsReplied)] = value == "是";
                        }
                        else if (StringUtils.EqualsIgnoreCase(columnName, "回复时间"))
                        {
                            dict[nameof(DataInfo.ReplyDate)] = TranslateUtils.ToDateTime(value);
                        }
                        else if (StringUtils.EqualsIgnoreCase(columnName, "回复内容"))
                        {
                            dict[nameof(DataInfo.ReplyContent)] = value;
                        }
                        else
                        {
                            var style = styles.FirstOrDefault(x =>
                                StringUtils.EqualsIgnoreCase(x.AttributeName, columnName) ||
                                StringUtils.EqualsIgnoreCase(x.DisplayName, columnName));
                            var attributeName = style != null ? style.AttributeName : columnName;

                            if (!string.IsNullOrEmpty(attributeName))
                            {
                                dict[attributeName] = value;
                            }
                        }
                    }

                    var dataInfo = new DataInfo();
                    dataInfo.LoadDict(dict);

                    dataInfo.FormId = request.FormId;
                    dataInfos.Add(dataInfo);
                }

                foreach (var dataInfo in dataInfos)
                {
                    await _dataRepository.InsertAsync(formInfo, dataInfo);
                }
            }

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
