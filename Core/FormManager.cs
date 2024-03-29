﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SSCMS.Configuration;
using SSCMS.Enums;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Models;
using SSCMS.Form.Utils;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Form.Core
{
    public partial class FormManager : IFormManager
    {
        private const string PluginId = "sscms.form";
        public const string PermissionsForms = "form_forms";
        public const string PermissionsTemplates = "form_templates";

        private readonly ICacheManager _cacheManager;
        private readonly ISettingsManager _settingsManager;
        private readonly IPathManager _pathManager;
        private readonly IPluginManager _pluginManager;
        private readonly ISmsManager _smsManager;
        private readonly IMailManager _mailManager;
        private readonly IFormRepository _formRepository;
        private readonly ITableStyleRepository _tableStyleRepository;
        private readonly IDataRepository _dataRepository;

        public FormManager(ICacheManager cacheManager, ISettingsManager settingsManager, IPathManager pathManager, ISmsManager smsManager, IMailManager mailManager, IPluginManager pluginManager, IFormRepository formRepository, ITableStyleRepository tableStyleRepository, IDataRepository dataRepository)
        {
            _cacheManager = cacheManager;
            _settingsManager = settingsManager;
            _pathManager = pathManager;
            _pluginManager = pluginManager;
            _smsManager = smsManager;
            _mailManager = mailManager;
            _formRepository = formRepository;
            _tableStyleRepository = tableStyleRepository;
            _dataRepository = dataRepository;
        }

        public const string DefaultListAttributeNames = "Name,Mobile,Email,Content";

        public List<ContentColumn> GetColumns(List<string> listAttributeNames, List<TableStyle> styles, bool isReply)
        {
            var columns = new List<ContentColumn>
            {
                new ContentColumn
                {
                    AttributeName = nameof(DataInfo.Id),
                    DisplayName = "Id",
                    IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, nameof(DataInfo.Id))
                },
                new ContentColumn
                {
                    AttributeName = nameof(DataInfo.Guid),
                    DisplayName = "编号",
                    IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, nameof(DataInfo.Guid))
                }
            };

            foreach (var style in styles)
            {
                if (string.IsNullOrEmpty(style.DisplayName) || style.InputType == InputType.TextEditor) continue;

                var column = new ContentColumn
                {
                    AttributeName = style.AttributeName,
                    DisplayName = style.DisplayName,
                    InputType = style.InputType,
                    IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, style.AttributeName)
                };

                columns.Add(column);
            }

            columns.AddRange(new List<ContentColumn>
            {
                new ContentColumn
                {
                    AttributeName = nameof(DataInfo.CreatedDate),
                    DisplayName = "添加时间",
                    IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, nameof(DataInfo.CreatedDate))
                },
                new ContentColumn
                {
                    AttributeName = nameof(DataInfo.LastModifiedDate),
                    DisplayName = "更新时间",
                    IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, nameof(DataInfo.LastModifiedDate))
                },
                new ContentColumn
                {
                    AttributeName = nameof(DataInfo.ChannelId),
                    DisplayName = "所在栏目页",
                    IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, nameof(DataInfo.ChannelId))
                },
                new ContentColumn
                {
                    AttributeName = nameof(DataInfo.ContentId),
                    DisplayName = "所在内容页",
                    IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, nameof(DataInfo.ContentId))
                }
            });

            if (isReply)
            {
                columns.AddRange(new List<ContentColumn>
                {
                    new ContentColumn
                    {
                        AttributeName = nameof(DataInfo.ReplyDate),
                        DisplayName = "回复时间",
                        IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, nameof(DataInfo.ReplyDate))
                    },
                    new ContentColumn
                    {
                        AttributeName = nameof(DataInfo.ReplyContent),
                        DisplayName = "回复内容",
                        IsList = ListUtils.ContainsIgnoreCase(listAttributeNames, nameof(DataInfo.ReplyContent))
                    }
                });
            }

            return columns;
        }

        public async Task<DataInfo> GetDataInfoAsync(int dataId, int formId, List<TableStyle> styles)
        {
            DataInfo dataInfo;
            if (dataId > 0)
            {
                dataInfo = await _dataRepository.GetDataInfoAsync(dataId);
            }
            else
            {
                dataInfo = new DataInfo
                {
                    FormId = formId
                };

                foreach (var style in styles)
                {
                    if (style.InputType == InputType.Text || style.InputType == InputType.TextArea || style.InputType == InputType.TextEditor || style.InputType == InputType.Hidden)
                    {
                        if (string.IsNullOrEmpty(style.DefaultValue)) continue;

                        dataInfo.Set(style.AttributeName, style.DefaultValue);
                    }
                    else if (style.InputType == InputType.Number)
                    {
                        if (string.IsNullOrEmpty(style.DefaultValue)) continue;

                        dataInfo.Set(style.AttributeName, TranslateUtils.ToInt(style.DefaultValue));
                    }
                    else if (style.InputType == InputType.CheckBox || style.InputType == InputType.SelectMultiple)
                    {
                        var value = new List<string>();

                        if (style.Items != null)
                        {
                            foreach (var item in style.Items)
                            {
                                if (item.Selected)
                                {
                                    value.Add(item.Value);
                                }
                            }
                        }

                        dataInfo.Set(style.AttributeName, value);
                    }
                    else if (style.InputType == InputType.Radio || style.InputType == InputType.SelectOne)
                    {
                        if (style.Items != null)
                        {
                            foreach (var item in style.Items)
                            {
                                if (item.Selected)
                                {
                                    dataInfo.Set(style.AttributeName, item.Value);
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(style.DefaultValue))
                        {
                            dataInfo.Set(style.AttributeName, style.DefaultValue);
                        }
                    }
                }
            }

            return dataInfo;
        }

        public async Task DeleteAsync(int siteId, int formId)
        {
            if (formId <= 0) return;

            var formInfo = await _formRepository.GetFormInfoAsync(siteId, formId);
            var relatedIdentities = GetRelatedIdentities(formInfo.Id);

            await _tableStyleRepository.DeleteAllAsync(FormUtils.TableNameData, relatedIdentities);
            await _dataRepository.DeleteByFormIdAsync(formId);
            await _formRepository.DeleteAsync(siteId, formId);
        }

        public async Task SendNotifyAsync(FormInfo formInfo, List<TableStyle> styles, DataInfo dataInfo)
        {
            if (formInfo.IsAdministratorSmsNotify &&
                !string.IsNullOrEmpty(formInfo.AdministratorSmsNotifyTplId) &&
                !string.IsNullOrEmpty(formInfo.AdministratorSmsNotifyMobile))
            {
                var isSmsEnabled = await _smsManager.IsSmsEnabledAsync();
                if (isSmsEnabled)
                {
                    var parameters = new Dictionary<string, string>();
                    if (!string.IsNullOrEmpty(formInfo.AdministratorSmsNotifyKeys))
                    {
                        var keys = formInfo.AdministratorSmsNotifyKeys.Split(',');
                        foreach (var key in keys)
                        {
                            if (StringUtils.EqualsIgnoreCase(key, nameof(DataInfo.Id)))
                            {
                                parameters.Add(key, dataInfo.Id.ToString());
                            }
                            else if (StringUtils.EqualsIgnoreCase(key, nameof(DataInfo.CreatedDate)))
                            {
                                if (dataInfo.CreatedDate.HasValue)
                                {
                                    parameters.Add(key, dataInfo.CreatedDate.Value.ToString("yyyy-MM-dd HH:mm"));
                                }
                            }
                            else
                            {
                                var value = dataInfo.Get<string>(key);
                                parameters.Add(key, value);
                            }
                        }
                    }

                    await _smsManager.SendSmsAsync(formInfo.AdministratorSmsNotifyMobile,
                        formInfo.AdministratorSmsNotifyTplId, parameters);
                }
            }

            if (formInfo.IsAdministratorMailNotify &&
                !string.IsNullOrEmpty(formInfo.AdministratorMailNotifyAddress))
            {
                var isMailEnabled = await _mailManager.IsMailEnabledAsync();
                if (isMailEnabled)
                {
                    var templateHtml = await GetMailTemplateHtmlAsync();
                    var listHtml = await GetMailListHtmlAsync();

                    var keyValueList = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("编号", dataInfo.Guid)
                    };
                    if (dataInfo.CreatedDate.HasValue)
                    {
                        keyValueList.Add(new KeyValuePair<string, string>("提交时间",
                            dataInfo.CreatedDate.Value.ToString("yyyy-MM-dd HH:mm")));
                    }

                    foreach (var style in styles)
                    {
                        keyValueList.Add(new KeyValuePair<string, string>(style.DisplayName,
                            dataInfo.Get<string>(style.AttributeName)));
                    }

                    var list = new StringBuilder();
                    foreach (var kv in keyValueList)
                    {
                        list.Append(listHtml.Replace("{{key}}", kv.Key).Replace("{{value}}", kv.Value));
                    }

                    var subject = !string.IsNullOrEmpty(formInfo.AdministratorMailTitle) ? formInfo.AdministratorMailTitle : "[SSCMS] 通知邮件";
                    var htmlBody = templateHtml
                        .Replace("{{title}}", formInfo.Title)
                        .Replace("{{list}}", list.ToString());

                    await _mailManager.SendMailAsync(formInfo.AdministratorMailNotifyAddress, subject,
                        htmlBody);
                }
            }

            if (formInfo.IsUserSmsNotify &&
                !string.IsNullOrEmpty(formInfo.UserSmsNotifyTplId) &&
                !string.IsNullOrEmpty(formInfo.UserSmsNotifyMobileName))
            {
                var isSmsEnabled = await _smsManager.IsSmsEnabledAsync();
                if (isSmsEnabled)
                {
                    var parameters = new Dictionary<string, string>();
                    if (!string.IsNullOrEmpty(formInfo.UserSmsNotifyKeys))
                    {
                        var keys = formInfo.UserSmsNotifyKeys.Split(',');
                        foreach (var key in keys)
                        {
                            if (StringUtils.EqualsIgnoreCase(key, nameof(DataInfo.Id)))
                            {
                                parameters.Add(key, dataInfo.Id.ToString());
                            }
                            else if (StringUtils.EqualsIgnoreCase(key, nameof(DataInfo.CreatedDate)))
                            {
                                if (dataInfo.CreatedDate.HasValue)
                                {
                                    parameters.Add(key, dataInfo.CreatedDate.Value.ToString("yyyy-MM-dd HH:mm"));
                                }
                            }
                            else
                            {
                                var value = dataInfo.Get<string>(key);
                                parameters.Add(key, value);
                            }
                        }
                    }

                    var mobile = dataInfo.Get<string>(formInfo.UserSmsNotifyMobileName);
                    if (!string.IsNullOrEmpty(mobile))
                    {
                        await _smsManager.SendSmsAsync(mobile, formInfo.UserSmsNotifyTplId, parameters);
                    }
                }
            }
        }

        public int GetPageSize(FormInfo formInfo)
        {
            if (formInfo == null || formInfo.PageSize <= 0) return 30;
            return formInfo.PageSize;
        }
    }
}
