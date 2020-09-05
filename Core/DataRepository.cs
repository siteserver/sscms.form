using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Datory;
using SSCMS.Enums;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Models;
using SSCMS.Form.Utils;
using SSCMS.Models;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Form.Core
{
    public class DataRepository : IDataRepository
    {
        private readonly Repository<DataInfo> _repository;
        private readonly IFormRepository _formRepository;

        public DataRepository(ISettingsManager settingsManager, IFormRepository formRepository)
        {
            _repository = new Repository<DataInfo>(settingsManager.Database, settingsManager.Redis);
            _formRepository = formRepository;
        }

        private static class Attr
        {
            public const string Id = nameof(DataInfo.Id);

            public const string FormId = nameof(DataInfo.FormId);
            public const string ExtendValues = nameof(ExtendValues);
        }

        public List<TableColumn> TableColumns => _repository.TableColumns;

        public async Task<int> InsertAsync(FormInfo formInfo, DataInfo dataInfo)
        {
            dataInfo.FormId = formInfo.Id;
            dataInfo.Id = await _repository.InsertAsync(dataInfo);

            formInfo.TotalCount += 1;
            await _formRepository.UpdateAsync(formInfo);

            return dataInfo.Id;
        }

        public async Task UpdateAsync(DataInfo dataInfo)
        {
            await _repository.UpdateAsync(dataInfo);
        }

        public async Task<DataInfo> GetDataInfoAsync(int dataId)
        {
            return await _repository.GetAsync(dataId);
        }

        public async Task ReplyAsync(FormInfo formInfo, DataInfo dataInfo)
        {
            await _repository.UpdateAsync(Q
                .Set(nameof(DataInfo.IsReplied), true)
                .Set(nameof(DataInfo.ReplyDate), DateTime.Now)
                .Set(nameof(DataInfo.ReplyContent), dataInfo.ReplyContent)
                .Where("Id", dataInfo.Id)
            );

            if (!dataInfo.IsReplied)
            {
                formInfo.RepliedCount += 1;
                await _formRepository.UpdateAsync(formInfo);
            }
        }

        public async Task DeleteByFormIdAsync(int formId)
        {
            if (formId <= 0) return;

            await _repository.DeleteAsync(Q.Where(Attr.FormId, formId));
        }

        public async Task DeleteAsync(FormInfo formInfo, DataInfo dataInfo)
        {
            await _repository.DeleteAsync(dataInfo.Id);

            if (dataInfo.IsReplied)
            {
                formInfo.RepliedCount -= 1;
            }
            formInfo.TotalCount -= 1;
            await _formRepository.UpdateAsync(formInfo);
        }

        public async Task<int> GetCountAsync(int formId)
        {
            return await _repository.CountAsync(Q.Where(Attr.FormId, formId));
        }

        public async Task<(int Total, List<DataInfo>)> GetDataAsync(FormInfo formInfo, bool isRepliedOnly, string word, int page, int pageSize)
        {
            if (formInfo.TotalCount == 0)
            {
                return (0, new List<DataInfo>());
            }

            if (page == 0) page = 1;

            var q = Q
                .Where(Attr.FormId, formInfo.Id)
                .OrderBy(nameof(DataInfo.IsReplied))
                .OrderByDesc(Attr.Id)
                .ForPage(page, pageSize);

            if (isRepliedOnly)
            {
                q.Where(nameof(DataInfo.IsReplied), true);
            }

            if (!string.IsNullOrEmpty(word))
            {
                q.Where(query => query
                    .WhereLike(Attr.ExtendValues, $"%{word}%")
                    .OrWhereLike(nameof(DataInfo.ReplyContent), $"%{word}%")
                );
            }

            var count = await _repository.CountAsync(q);
            var list = await _repository.GetAllAsync(q);

            return (count, list);
        }

        public async Task<IList<DataInfo>> GetAllDataInfoListAsync(FormInfo formInfo)
        {
            if (formInfo.TotalCount == 0)
            {
                return new List<DataInfo>();
            }

            var q = Q
                .Where(Attr.FormId, formInfo.Id)
                .OrderBy(nameof(DataInfo.IsReplied))
                .OrderByDesc(Attr.Id);

            return await _repository.GetAllAsync(q);
        }

        public string GetValue(TableStyle style, DataInfo dataInfo)
        {
            var value = string.Empty;
            if (dataInfo.ContainsKey(style.AttributeName))
            {
                var fieldValue = dataInfo.Get<string>(style.AttributeName);

                if (style.InputType == InputType.CheckBox || style.InputType == InputType.SelectMultiple)
                {
                    var list = TranslateUtils.JsonDeserialize<List<string>>(fieldValue);
                    if (list != null)
                    {
                        value = string.Join(",", list);
                    }
                }
                else if (style.InputType == InputType.Date)
                {
                    if (!string.IsNullOrEmpty(fieldValue))
                    {
                        var date = FormUtils.ToDateTime(fieldValue, DateTime.MinValue);
                        if (date != DateTime.MinValue)
                        {
                            value = date.ToString("yyyy-MM-dd");
                        }
                    }
                }
                else if (style.InputType == InputType.DateTime)
                {
                    if (!string.IsNullOrEmpty(fieldValue))
                    {
                        var date = FormUtils.ToDateTime(fieldValue, DateTime.MinValue);
                        if (date != DateTime.MinValue)
                        {
                            value = date.ToString("yyyy-MM-dd HH:mm");
                        }
                    }
                }
                else
                {
                    value = fieldValue;
                }
            }

            return value;
        }
    }
}
