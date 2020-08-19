using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Datory;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Models;
using SSCMS.Models;
using SSCMS.Services;

namespace SSCMS.Form.Core
{
    public class FormRepository : IFormRepository
    {
        private readonly Repository<FormInfo> _repository;

        public FormRepository(ISettingsManager settingsManager)
        {
            _repository = new Repository<FormInfo>(settingsManager.Database, settingsManager.Redis);
        }

        private static string GetCacheKey(int siteId)
        {
            return $"SSCMS.Form.Core.Repositories.FormRepository:{siteId}";
        }

        public List<TableColumn> TableColumns => _repository.TableColumns;

        public async Task<int> InsertAsync(FormInfo formInfo)
        {
            if (formInfo.SiteId == 0) return 0;
            if (formInfo.ChannelId == 0 && formInfo.ContentId == 0 && string.IsNullOrEmpty(formInfo.Title)) return 0;

            if (formInfo.ContentId == 0)
            {
                formInfo.Taxis = await GetMaxTaxisAsync(formInfo.SiteId) + 1;
            }

            formInfo.Id = await _repository.InsertAsync(formInfo, Q.CachingRemove(GetCacheKey(formInfo.SiteId)));

            return formInfo.Id;
        }

        public async Task<bool> UpdateAsync(FormInfo formInfo)
        {
            var updated = await _repository.UpdateAsync(formInfo, Q.CachingRemove(GetCacheKey(formInfo.SiteId)));

            return updated;
        }

        public async Task DeleteAsync(int siteId, int formId)
        {
            if (formId <= 0) return;

            await _repository.DeleteAsync(formId, Q.CachingRemove(GetCacheKey(siteId)));
        }

        public async Task UpdateTaxisToDownAsync(int siteId, int formId)
        {
            var taxis = await _repository.GetAsync<int>(Q
                .Select(nameof(FormInfo.Taxis))
                .Where(nameof(FormInfo.Id), formId)
            );

            var dataInfo = await _repository.GetAsync(Q
                .Where(nameof(FormInfo.SiteId), siteId)
                .Where(nameof(FormInfo.Taxis), ">", taxis)
                .OrderBy(nameof(FormInfo.Taxis))
            );

            if (dataInfo == null) return;

            var higherId = dataInfo.Id;
            var higherTaxis = dataInfo.Taxis;

            await SetTaxisAsync(siteId, formId, higherTaxis);
            await SetTaxisAsync(siteId, higherId, taxis);
        }

        public async Task UpdateTaxisToUpAsync(int siteId, int formId)
        {
            var taxis = await _repository.GetAsync<int>(Q
                .Select(nameof(FormInfo.Taxis))
                .Where(nameof(FormInfo.Id), formId)
            );

            var dataInfo = await _repository.GetAsync(Q
                .Where(nameof(FormInfo.SiteId), siteId)
                .Where(nameof(FormInfo.Taxis), "<", taxis)
                .OrderByDesc(nameof(FormInfo.Taxis))
            );

            if (dataInfo == null) return;

            var lowerId = dataInfo.Id;
            var lowerTaxis = dataInfo.Taxis;

            await SetTaxisAsync(siteId, formId, lowerTaxis);
            await SetTaxisAsync(siteId, lowerId, taxis);
        }

        private async Task<int> GetMaxTaxisAsync(int siteId)
        {
            return await _repository.MaxAsync(nameof(FormInfo.Taxis), Q
                .Where(nameof(FormInfo.SiteId), siteId)
            ) ?? 0;
        }

        private async Task SetTaxisAsync(int siteId, int formId, int taxis)
        {
            await _repository.UpdateAsync(Q
                .Set(nameof(FormInfo.Taxis), taxis)
                .Where(nameof(FormInfo.Id), formId)
                .CachingRemove(GetCacheKey(siteId))
            );
        }

        public async Task<IList<FormInfo>> GetFormInfoListAsync(int siteId)
        {
            return await _repository.GetAllAsync(Q
                .Where(nameof(FormInfo.SiteId), siteId)
                .OrderBy(nameof(FormInfo.Taxis), nameof(FormInfo.Id))
                .CachingGet(GetCacheKey(siteId))
            );
        }

        public async Task<string> GetImportTitleAsync(int siteId, string title)
        {
            string importTitle;
            if (title.IndexOf("_", StringComparison.Ordinal) != -1)
            {
                var inputNameCount = 0;
                var lastInputName = title.Substring(title.LastIndexOf("_", StringComparison.Ordinal) + 1);
                var firstInputName = title.Substring(0, title.Length - lastInputName.Length);
                try
                {
                    inputNameCount = int.Parse(lastInputName);
                }
                catch
                {
                    // ignored
                }
                inputNameCount++;
                importTitle = firstInputName + inputNameCount;
            }
            else
            {
                importTitle = title + "_1";
            }

            var inputInfo = await GetFormInfoByTitleAsync(siteId, title);
            if (inputInfo != null)
            {
                importTitle = await GetImportTitleAsync(siteId, importTitle);
            }

            return importTitle;
        }

        public async Task<List<FormInfo>> GetFormInfoListAsync(int siteId, int channelId)
        {
            var formInfoList = await GetFormInfoListAsync(siteId);

            return formInfoList
                .Where(formInfo => formInfo.ChannelId == channelId)
                .OrderBy(formInfo => formInfo.Taxis == 0 ? int.MaxValue : formInfo.Taxis)
                .ToList();
        }

        public async Task<FormInfo> GetFormInfoAsync(int siteId, int id)
        {
            var formInfoList = await GetFormInfoListAsync(siteId);

            return formInfoList.FirstOrDefault(x => x.Id == id);
        }

        public async Task<FormInfo> GetFormInfoByContentIdAsync(int siteId, int channelId, int contentId)
        {
            var formInfoList = await GetFormInfoListAsync(siteId);
            return formInfoList.FirstOrDefault(x => x.ChannelId == channelId && x.ContentId == contentId);
        }

        public async Task<FormInfo> GetFormInfoByTitleAsync(int siteId, string title)
        {
            var formInfoList = await GetFormInfoListAsync(siteId);
            return formInfoList.FirstOrDefault(x => x.Title == title);
        }

        public List<string> GetAllAttributeNames(List<TableStyle> styles)
        {
            var allAttributeNames = new List<string>
            {
                nameof(DataInfo.Id),
                nameof(DataInfo.Guid)
            };
            foreach (var style in styles)
            {
                allAttributeNames.Add(style.AttributeName);
            }
            allAttributeNames.Add(nameof(DataInfo.CreatedDate));
            allAttributeNames.Add(nameof(DataInfo.LastModifiedDate));

            return allAttributeNames;
        }

        public string GetFormTitle(FormInfo formInfo)
        {
            var text = "表单管理 (0)";
            if (formInfo == null) return text;

            text = $"{(formInfo.ContentId > 0 ? "表单管理" : formInfo.Title)} ({formInfo.TotalCount})";
            if (!formInfo.IsReply) return text;

            if (formInfo.TotalCount - formInfo.RepliedCount > 0)
            {
                text = $@"<span class=""text-danger"">{text}</span>";
            }

            return text;
        }
    }
}
