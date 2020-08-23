using System.Collections.Generic;
using System.Threading.Tasks;
using SSCMS.Form.Core;
using SSCMS.Form.Models;
using SSCMS.Form.Utils.Atom.Atom.AdditionalElements;
using SSCMS.Models;

namespace SSCMS.Form.Abstractions
{
    public interface IFormManager
    {
        Task<FormInfo> GetFormInfoByRequestAsync(int siteId, int channelId, int contentId, int formId);

        Task CreateDefaultStylesAsync(FormInfo formInfo);

        Task DeleteAsync(int siteId, int formId);

        //Task TranslateAsync(int siteId, int channelId, int contentId, int targetSiteId, int targetChannelId,
        //    int targetContentId);

        Task ImportFormAsync(int siteId, string directoryPath, bool overwrite);

        Task ExportFormAsync(int siteId, string directoryPath, int formId);

        void AddDcElement(ScopedElementCollection collection, List<string> nameList, string content);

        string GetDcElementContent(ScopedElementCollection additionalElements, List<string> nameList);

        Task<string> GetMailTemplateHtmlAsync();

        Task<string> GetMailListHtmlAsync();

        void SendNotify(FormInfo formInfo, List<TableStyle> styles, DataInfo dataInfo);

        List<TemplateInfo> GetTemplateInfoList(string type);

        TemplateInfo GetTemplateInfo(string name);

        void Clone(string nameToClone, TemplateInfo templateInfo, string templateHtml = null);

        void Edit(TemplateInfo templateInfo);

        Task<string> GetTemplateHtmlAsync(TemplateInfo templateInfo);

        void SetTemplateHtml(TemplateInfo templateInfo, string html);

        void DeleteTemplate(string name);

        string GetTableName(FormRequest request);

        string GetTableName(FormInfo formInfo);

        List<int> GetRelatedIdentities(FormRequest request);

        List<int> GetRelatedIdentities(FormInfo formInfo);

        Task DeleteTableStyleAsync(string tableName, List<int> relatedIdentities, string attributeName);

        Task<List<TableStyle>> GetTableStylesAsync(string tableName, List<int> relatedIdentities);

        int GetPageSize(FormInfo formInfo);
    }
}
