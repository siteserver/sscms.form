using System.Collections.Generic;
using System.Threading.Tasks;
using SSCMS.Configuration;
using SSCMS.Form.Models;
using SSCMS.Models;

namespace SSCMS.Form.Abstractions
{
    public interface IFormManager
    {
        Task CreateDefaultStylesAsync(FormInfo formInfo);

        List<ContentColumn> GetColumns(List<string> listAttributeNames, List<TableStyle> styles, bool isReply);

        Task<DataInfo> GetDataInfoAsync(int dataId, int formId, List<TableStyle> styles);

        Task DeleteAsync(int siteId, int formId);

        Task ImportFormAsync(int siteId, string directoryPath, bool overwrite);

        Task ExportFormAsync(int siteId, string directoryPath, int formId);

        Task<string> GetMailTemplateHtmlAsync();

        Task<string> GetMailListHtmlAsync();

        void SendNotify(FormInfo formInfo, List<TableStyle> styles, DataInfo dataInfo);

        List<TemplateInfo> GetTemplateInfoList(string type);

        TemplateInfo GetTemplateInfo(string name);

        void Clone(string nameToClone, TemplateInfo templateInfo, string templateHtml = null);

        void Edit(TemplateInfo templateInfo);

        string GetTemplateHtml(TemplateInfo templateInfo);

        void SetTemplateHtml(TemplateInfo templateInfo, string html);

        void DeleteTemplate(string name);

        List<int> GetRelatedIdentities(int formId);

        Task DeleteTableStyleAsync(int formId, string attributeName);

        Task<List<TableStyle>> GetTableStylesAsync(int formId);

        int GetPageSize(FormInfo formInfo);
    }
}
