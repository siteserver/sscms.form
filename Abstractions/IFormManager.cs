using System.Collections.Generic;
using System.Threading.Tasks;
using SSCMS.Configuration;
using SSCMS.Form.Models;
using SSCMS.Form.Utils.Atom.Atom.AdditionalElements;
using SSCMS.Models;

namespace SSCMS.Form.Abstractions
{
    public interface IFormManager
    {
        Task<FormInfo> GetFormInfoByRequestAsync(int siteId, int channelId, int contentId, int formId);

        Task CreateDefaultStylesAsync(FormInfo formInfo);

        List<ContentColumn> GetColumns(List<string> listAttributeNames, List<TableStyle> styles);

        Task<DataInfo> GetDataInfoAsync(int dataId, int formId, List<TableStyle> styles);

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

        List<int> GetRelatedIdentities(int formId);

        Task DeleteTableStyleAsync(int formId, string attributeName);

        Task<List<TableStyle>> GetTableStylesAsync(int formId);

        int GetPageSize(FormInfo formInfo);
    }
}
