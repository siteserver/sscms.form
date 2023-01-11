using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Datory;
using SSCMS.Form.Models;
using SSCMS.Models;

namespace SSCMS.Form.Abstractions
{
    public interface IDataRepository
    {
        List<TableColumn> TableColumns { get; }
        Task<int> InsertAsync(FormInfo formInfo, DataInfo dataInfo);

        Task UpdateAsync(DataInfo dataInfo);

        Task<DataInfo> GetDataInfoAsync(int dataId);

        Task ReplyAsync(FormInfo formInfo, DataInfo dataInfo);

        Task DeleteByFormIdAsync(int formId);

        Task DeleteAsync(FormInfo formInfo, DataInfo dataInfo);

        Task<int> GetCountAsync(int formId);

        Task<(int Total, List<DataInfo>)> GetListAsync(FormInfo formInfo, bool isRepliedOnly, DateTime? startDate, DateTime? endDate, string word, int page, int pageSize);

        Task<IList<DataInfo>> GetListAsync(FormInfo formInfo);

        string GetValue(TableStyle style, DataInfo dataInfo);
    }
}
