using System.Collections.Generic;
using System.Threading.Tasks;
using Datory;
using SSCMS.Form.Models;
using SSCMS.Models;

namespace SSCMS.Form.Abstractions
{
    public interface IFormRepository
    {
        List<TableColumn> TableColumns { get; }

        Task<int> InsertAsync(FormInfo formInfo);

        Task<bool> UpdateAsync(FormInfo formInfo);

        Task DeleteAsync(int siteId, int formId);

        Task UpdateTaxisToDownAsync(int siteId, int formId);

        Task UpdateTaxisToUpAsync(int siteId, int formId);

        Task<IList<FormInfo>> GetFormInfoListAsync(int siteId);

        Task<string> GetImportTitleAsync(int siteId, string title);

        Task<List<FormInfo>> GetFormInfoListAsync(int siteId, int channelId);

        Task<FormInfo> GetFormInfoAsync(int siteId, int id);

        Task<FormInfo> GetFormInfoByContentIdAsync(int siteId, int channelId, int contentId);

        Task<FormInfo> GetFormInfoByTitleAsync(int siteId, string title);

        List<string> GetAllAttributeNames(List<TableStyle> styles);

        string GetFormTitle(FormInfo formInfo);
    }
}
