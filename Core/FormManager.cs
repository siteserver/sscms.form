using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Datory;
using SSCMS.Configuration;
using SSCMS.Enums;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Models;
using SSCMS.Form.Utils;
using SSCMS.Form.Utils.Atom.Atom.AdditionalElements;
using SSCMS.Form.Utils.Atom.Atom.AdditionalElements.DublinCore;
using SSCMS.Form.Utils.Atom.Atom.Core;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Form.Core
{
    public class FormManager : IFormManager
    {
        public const string PermissionsForms = "form_forms";
        public const string PermissionsTemplates = "form_templates";

        private readonly ICacheManager<string> _cacheManager;
        private readonly IPathManager _pathManager;
        private readonly IPlugin _plugin;
        private readonly IFormRepository _formRepository;
        private readonly ITableStyleRepository _tableStyleRepository;
        private readonly IDataRepository _logRepository;

        public FormManager(ICacheManager<string> cacheManager, IPathManager pathManager, IPluginManager pluginManager, IFormRepository formRepository, ITableStyleRepository tableStyleRepository, IDataRepository logRepository)
        {
            _cacheManager = cacheManager;
            _pathManager = pathManager;
            _plugin = pluginManager.Current;
            _formRepository = formRepository;
            _tableStyleRepository = tableStyleRepository;
            _logRepository = logRepository;
        }

        public async Task<FormInfo> GetFormInfoByRequestAsync(int siteId, int channelId, int contentId, int formId)
        {
            return formId > 0 ? await _formRepository.GetFormInfoAsync(siteId, formId) : await GetFormInfoOrCreateIfNotExistsAsync(siteId, channelId, contentId);
        }

        private async Task<FormInfo> GetFormInfoOrCreateIfNotExistsAsync(int siteId, int channelId, int contentId)
        {
            var formInfo = await _formRepository.GetFormInfoByContentIdAsync(siteId, channelId, contentId);

            if (formInfo == null)
            {
                formInfo = new FormInfo
                {
                    SiteId = siteId,
                    ChannelId = channelId,
                    ContentId = contentId,
                    Title = "默认表单",
                    Description = string.Empty,
                    IsReply = false,
                    RepliedCount = 0,
                    TotalCount = 0,
                    ListAttributeNames = DefaultListAttributeNames
                };
                formInfo.Id = await _formRepository.InsertAsync(formInfo);

                await CreateDefaultStylesAsync(formInfo);
            }

            return formInfo;
        }

        public const string DefaultListAttributeNames = "Name,Mobile,Email,Content";

        public async Task CreateDefaultStylesAsync(FormInfo formInfo)
        {
            var tableName = GetTableName(formInfo);
            var relatedIdentities = GetRelatedIdentities(formInfo);

            await _tableStyleRepository.InsertAsync(relatedIdentities, new TableStyle
            {
                TableName = tableName,
                RelatedIdentity = relatedIdentities[0],
                AttributeName = "Name",
                DisplayName = "姓名",
                HelpText = "请输入您的姓名",
                InputType = InputType.Text,
                Rules = new List<InputStyleRule>
                {
                    new InputStyleRule
                    {
                        Type = ValidateType.Required,
                        Message = ValidateType.Required.GetDisplayName()
                    }
                }
            });

            await _tableStyleRepository.InsertAsync(relatedIdentities, new TableStyle
            {
                TableName = tableName,
                RelatedIdentity = relatedIdentities[0],
                AttributeName = "Mobile",
                DisplayName = "手机",
                HelpText = "请输入您的手机号码",
                InputType = InputType.Text,
                Rules = new List<InputStyleRule>
                {
                    new InputStyleRule
                    {
                        Type = ValidateType.Mobile,
                        Message = ValidateType.Mobile.GetDisplayName()
                    }
                }
            });

            await _tableStyleRepository.InsertAsync(relatedIdentities, new TableStyle
            {
                TableName = tableName,
                RelatedIdentity = relatedIdentities[0],
                AttributeName = "Email",
                DisplayName = "邮箱",
                HelpText = "请输入您的电子邮箱",
                InputType = InputType.Text,
                Rules = new List<InputStyleRule>
                {
                    new InputStyleRule
                    {
                        Type = ValidateType.Email,
                        Message = ValidateType.Email.GetDisplayName()
                    }
                }
            });

            await _tableStyleRepository.InsertAsync(relatedIdentities, new TableStyle
            {
                TableName = tableName,
                RelatedIdentity = relatedIdentities[0],
                AttributeName = "Content",
                DisplayName = "留言",
                HelpText = "请输入您的留言",
                InputType = InputType.TextArea,
                Rules = new List<InputStyleRule>
                {
                    new InputStyleRule
                    {
                        Type = ValidateType.Required,
                        Message = ValidateType.Required.GetDisplayName()
                    }
                }
            });
        }

        public async Task DeleteAsync(int siteId, int formId)
        {
            if (formId <= 0) return;

            var formInfo = await _formRepository.GetFormInfoAsync(siteId, formId);
            var tableName = GetTableName(formInfo);
            var relatedIdentities = GetRelatedIdentities(formInfo);

            await _tableStyleRepository.DeleteAllAsync(tableName, relatedIdentities);
            await _logRepository.DeleteByFormIdAsync(formId);
            await _formRepository.DeleteAsync(siteId, formId);
        }

        private const string VersionFileName = "version.txt";

        private bool IsHistoric(string directoryPath)
        {
            if (!FileUtils.IsFileExists(PathUtils.Combine(directoryPath, VersionFileName))) return true;

            FileUtils.DeleteFileIfExists(PathUtils.Combine(directoryPath, VersionFileName));

            return false;
        }

        public async Task ImportFormAsync(int siteId, string directoryPath, bool overwrite)
        {
            if (!Directory.Exists(directoryPath)) return;
            var isHistoric = IsHistoric(directoryPath);

            var filePaths = Directory.GetFiles(directoryPath);

            foreach (var filePath in filePaths)
            {
                var feed = AtomFeed.Load(new FileStream(filePath, FileMode.Open));

                var formInfo = new FormInfo();

                foreach (var tableColumn in _formRepository.TableColumns)
                {
                    var value = GetValue(feed.AdditionalElements, tableColumn);
                    formInfo.Set(tableColumn.AttributeName, value);
                }

                formInfo.SiteId = siteId;

                if (isHistoric)
                {
                    formInfo.Title = GetDcElementContent(feed.AdditionalElements, "InputName");
                }

                var srcFormInfo = await _formRepository.GetFormInfoByTitleAsync(siteId, formInfo.Title);
                if (srcFormInfo != null)
                {
                    if (overwrite)
                    {
                        await DeleteAsync(siteId, srcFormInfo.Id);
                    }
                    else
                    {
                        formInfo.Title = await _formRepository.GetImportTitleAsync(siteId, formInfo.Title);
                    }
                }

                formInfo.Id = await _formRepository.InsertAsync(formInfo);

                var directoryName = GetDcElementContent(feed.AdditionalElements, "Id");
                if (isHistoric)
                {
                    directoryName = GetDcElementContent(feed.AdditionalElements, "InputID");
                }
                var titleAttributeNameDict = new NameValueCollection();
                if (!string.IsNullOrEmpty(directoryName))
                {
                    var fieldDirectoryPath = PathUtils.Combine(directoryPath, directoryName);
                    titleAttributeNameDict = await ImportFieldsAsync(siteId, formInfo.Id, fieldDirectoryPath, isHistoric);
                }

                var entryList = new List<AtomEntry>();
                foreach (AtomEntry entry in feed.Entries)
                {
                    entryList.Add(entry);
                }

                entryList.Reverse();

                foreach (var entry in entryList)
                {
                    var dataInfo = new DataInfo();

                    foreach (var tableColumn in _logRepository.TableColumns)
                    {
                        var value = GetValue(entry.AdditionalElements, tableColumn);
                        dataInfo.Set(tableColumn.AttributeName, value);
                    }

                    var attributes = GetDcElementNameValueCollection(entry.AdditionalElements);
                    foreach (string entryName in attributes.Keys)
                    {
                        dataInfo.Set(entryName, attributes[entryName]);
                    }

                    if (isHistoric)
                    {
                        foreach (var title in titleAttributeNameDict.AllKeys)
                        {
                            dataInfo.Set(title, dataInfo.Get(titleAttributeNameDict[title]));
                        }

                        dataInfo.ReplyContent = GetDcElementContent(entry.AdditionalElements, "Reply");
                        if (!string.IsNullOrEmpty(dataInfo.ReplyContent))
                        {
                            dataInfo.IsReplied = true;
                        }
                        dataInfo.CreatedDate = FormUtils.ToDateTime(GetDcElementContent(entry.AdditionalElements, "adddate"));
                    }

                    await _logRepository.InsertAsync(formInfo, dataInfo);
                }
            }
        }

        private async Task<NameValueCollection> ImportFieldsAsync(int siteId, int formId, string styleDirectoryPath, bool isHistoric)
        {
            var titleAttributeNameDict = new NameValueCollection();

            if (!Directory.Exists(styleDirectoryPath)) return titleAttributeNameDict;

            var formInfo = await _formRepository.GetFormInfoAsync(siteId, formId);
            var tableName = GetTableName(formInfo);
            var relatedIdentities = GetRelatedIdentities(formInfo);
            await _pathManager.ImportStylesAsync(tableName, relatedIdentities, styleDirectoryPath);

            //var filePaths = Directory.GetFiles(styleDirectoryPath);
            //foreach (var filePath in filePaths)
            //{
            //    var feed = AtomFeed.Load(new FileStream(filePath, FileMode.Open));

            //    var attributeName = GetDcElementContent(feed.AdditionalElements, "AttributeName");
            //    var title = GetDcElementContent(feed.AdditionalElements, "DisplayName");
            //    if (isHistoric)
            //    {
            //        title = GetDcElementContent(feed.AdditionalElements, "DisplayName");

            //        titleAttributeNameDict[title] = attributeName;
            //    }
            //    var fieldType = GetDcElementContent(feed.AdditionalElements, nameof(TableStyle.InputType));
            //    if (isHistoric)
            //    {
            //        fieldType = GetDcElementContent(feed.AdditionalElements, "InputType");
            //    }
            //    var taxis = FormUtils.ToIntWithNegative(GetDcElementContent(feed.AdditionalElements, "Taxis"), 0);

            //    var style = new TableStyle
            //    {
            //        TableName = tableName,
            //        RelatedIdentity = relatedIdentities[0],
            //        Taxis = taxis,
            //        Title = title,
            //        InputType = TranslateUtils.ToEnum(fieldType, InputType.Text)
            //    };

            //    var fieldItems = new List<FieldItemInfo>();
            //    foreach (AtomEntry entry in feed.Entries)
            //    {
            //        var itemValue = GetDcElementContent(entry.AdditionalElements, "ItemValue");
            //        var isSelected = FormUtils.ToBool(GetDcElementContent(entry.AdditionalElements, "IsSelected"), false);

            //        fieldItems.Add(new FieldItemInfo
            //        {
            //            FormId = formId,
            //            FieldId = 0,
            //            Value = itemValue,
            //            IsSelected = isSelected
            //        });
            //    }

            //    if (fieldItems.Count > 0)
            //    {
            //        style.Items = fieldItems;
            //    }

            //    if (await _fieldRepository.IsTitleExistsAsync(formId, title))
            //    {
            //        await _fieldRepository.DeleteAsync(formId, title);
            //    }
            //    await _fieldRepository.InsertAsync(siteId, style);
            //}

            return titleAttributeNameDict;
        }

        public async Task ExportFormAsync(int siteId, string directoryPath, int formId)
        {
            var formInfo = await _formRepository.GetFormInfoAsync(siteId, formId);
            var filePath = PathUtils.Combine(directoryPath, formInfo.Id + ".xml");

            var feed = GetEmptyFeed();

            foreach (var tableColumn in _formRepository.TableColumns)
            {
                SetValue(feed.AdditionalElements, tableColumn, formInfo);
            }

            var styleDirectoryPath = PathUtils.Combine(directoryPath, formInfo.Id.ToString());

            var tableName = GetTableName(formInfo);
            var relatedIdentities = GetRelatedIdentities(formInfo);

            await _pathManager.ExportStylesAsync(siteId, tableName, relatedIdentities);
            //await ExportFieldsAsync(formInfo.Id, styleDirectoryPath);

            var dataInfoList = await _logRepository.GetAllDataInfoListAsync(formInfo);
            foreach (var dataInfo in dataInfoList)
            {
                var entry = GetAtomEntry(dataInfo);
                feed.Entries.Add(entry);
            }
            feed.Save(filePath);

            await FileUtils.WriteTextAsync(PathUtils.Combine(directoryPath, VersionFileName), _plugin.Version);
        }

        //private AtomFeed ExportFieldInfo(FieldInfo style)
        //{
        //    var feed = GetEmptyFeed();

        //    foreach (var tableColumn in _fieldRepository.TableColumns)
        //    {
        //        SetValue(feed.AdditionalElements, tableColumn, style);
        //    }

        //    return feed;
        //}

        //private async Task ExportFieldsAsync(int formId, string styleDirectoryPath)
        //{

        //    DirectoryUtils.DeleteDirectoryIfExists(styleDirectoryPath);
        //    DirectoryUtils.CreateDirectoryIfNotExists(styleDirectoryPath);

        //    var styleList = await _fieldRepository.GetFieldInfoListAsync(formId);
        //    foreach (var style in styleList)
        //    {
        //        var filePath = PathUtils.Combine(styleDirectoryPath, style.Id + ".xml");
        //        var feed = ExportFieldInfo(style);
        //        if (style.Items != null && style.Items.Count > 0)
        //        {
        //            foreach (var itemInfo in style.Items)
        //            {
        //                var entry = ExportTableStyleItemInfo(itemInfo);
        //                feed.Entries.Add(entry);
        //            }
        //        }
        //        feed.Save(filePath);
        //    }
        //}

        //private AtomEntry ExportTableStyleItemInfo(FieldItemInfo styleItemInfo)
        //{
        //    var entry = GetEmptyEntry();

        //    foreach (var tableColumn in _fieldItemRepository.TableColumns)
        //    {
        //        SetValue(entry.AdditionalElements, tableColumn, styleItemInfo);
        //    }

        //    return entry;
        //}

        private const string Prefix = "SiteServer_";

        private string ToXmlContent(string inputString)
        {
            var contentBuilder = new StringBuilder(inputString);
            contentBuilder.Replace("<![CDATA[", string.Empty);
            contentBuilder.Replace("]]>", string.Empty);
            contentBuilder.Insert(0, "<![CDATA[");
            contentBuilder.Append("]]>");
            return contentBuilder.ToString();
        }

        private void AddDcElement(ScopedElementCollection collection, string name, string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                collection.Add(new DcElement(Prefix + name, ToXmlContent(content)));
            }
        }

        public void AddDcElement(ScopedElementCollection collection, List<string> nameList, string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                foreach (var name in nameList)
                {
                    collection.Add(new DcElement(Prefix + name, ToXmlContent(content)));
                }
            }
        }

        public string GetDcElementContent(ScopedElementCollection additionalElements, List<string> nameList)
        {
            return GetDcElementContent(additionalElements, nameList, "");
        }

        private string GetDcElementContent(ScopedElementCollection additionalElements, string name, string defaultContent = "")
        {
            var localName = Prefix + name;
            var element = additionalElements.FindScopedElementByLocalName(localName);
            return element != null ? element.Content : defaultContent;
        }

        private string GetDcElementContent(ScopedElementCollection additionalElements, List<string> nameList, string defaultContent)
        {
            foreach (var name in nameList)
            {
                var localName = Prefix + name;
                var element = additionalElements.FindScopedElementByLocalName(localName);
                if (element == null) continue;

                return element.Content;
            }
            return defaultContent;
        }

        private NameValueCollection GetDcElementNameValueCollection(ScopedElementCollection additionalElements)
        {
            return additionalElements.GetNameValueCollection(Prefix);
        }

        private AtomFeed GetEmptyFeed()
        {
            var feed = new AtomFeed
            {
                Title = new AtomContentConstruct("title", "siteserver channel"),
                Author = new AtomPersonConstruct("author",
                    "siteserver", new Uri("https://sscms.com")),
                Modified = new AtomDateConstruct("modified", DateTime.Now,
                    DateTimeOffset.UtcNow.Offset)
            };

            return feed;
        }

        private AtomEntry GetEmptyEntry()
        {
            var entry = new AtomEntry
            {
                Id = new Uri("https://sscms.com/"),
                Title = new AtomContentConstruct("title", "title"),
                Modified = new AtomDateConstruct("modified", DateTime.Now,
                    DateTimeOffset.UtcNow.Offset),
                Issued = new AtomDateConstruct("issued", DateTime.Now,
                    DateTimeOffset.UtcNow.Offset)
            };

            return entry;
        }

        private string Encrypt(string inputString)
        {
            return TranslateUtils.EncryptStringBySecretKey(inputString, "TgQQk42O");
        }

        private string Decrypt(string inputString)
        {
            return TranslateUtils.DecryptStringBySecretKey(inputString, "TgQQk42O");
        }

        private AtomEntry GetAtomEntry(Entity entity)
        {
            var entry = GetEmptyEntry();

            foreach (var keyValuePair in entity.ToDictionary())
            {
                if (keyValuePair.Value != null)
                {
                    AddDcElement(entry.AdditionalElements, keyValuePair.Key, keyValuePair.Value.ToString());
                }
            }

            return entry;
        }

        private object GetValue(ScopedElementCollection additionalElements, TableColumn tableColumn)
        {
            if (tableColumn.DataType == DataType.Boolean)
            {
                return TranslateUtils.ToBool(GetDcElementContent(additionalElements, tableColumn.AttributeName), false);
            }
            if (tableColumn.DataType == DataType.DateTime)
            {
                return FormUtils.ToDateTime(GetDcElementContent(additionalElements, tableColumn.AttributeName));
            }
            if (tableColumn.DataType == DataType.Decimal)
            {
                return FormUtils.ToDecimalWithNegative(GetDcElementContent(additionalElements, tableColumn.AttributeName), 0);
            }
            if (tableColumn.DataType == DataType.Integer)
            {
                return FormUtils.ToIntWithNegative(GetDcElementContent(additionalElements, tableColumn.AttributeName), 0);
            }
            if (tableColumn.DataType == DataType.Text)
            {
                return Decrypt(GetDcElementContent(additionalElements, tableColumn.AttributeName));
            }
            return GetDcElementContent(additionalElements, tableColumn.AttributeName);
        }

        private void SetValue(ScopedElementCollection additionalElements, TableColumn tableColumn, Entity entity)
        {
            var value = entity.Get(tableColumn.AttributeName)?.ToString();
            if (tableColumn.DataType == DataType.Text)
            {
                value = Encrypt(value);
            }
            AddDcElement(additionalElements, tableColumn.AttributeName, value);
        }

        private string GetMailTemplatesDirectoryPath()
        {
            return PathUtils.Combine(_plugin.WebRootPath, "assets/form/mail");
        }

        public async Task<string> GetMailTemplateHtmlAsync()
        {
            var directoryPath = GetMailTemplatesDirectoryPath();
            var htmlPath = PathUtils.Combine(directoryPath, "template.html");
            if (_cacheManager.Exists(htmlPath)) return _cacheManager.Get(htmlPath);

            var html = await FileUtils.ReadTextAsync(htmlPath);

            _cacheManager.AddOrUpdate(htmlPath, html);
            return html;
        }

        public async Task<string> GetMailListHtmlAsync()
        {
            var directoryPath = GetMailTemplatesDirectoryPath();
            var htmlPath = PathUtils.Combine(directoryPath, "list.html");
            if (_cacheManager.Exists(htmlPath)) return _cacheManager.Get(htmlPath);

            var html = await FileUtils.ReadTextAsync(htmlPath);

            _cacheManager.AddOrUpdate(htmlPath, html);
            return html;
        }

        public void SendNotify(FormInfo formInfo, List<TableStyle> styles, DataInfo dataInfo)
        {
            //TODO
            //if (formInfo.IsAdministratorSmsNotify &&
            //    !string.IsNullOrEmpty(formInfo.AdministratorSmsNotifyTplId) &&
            //    !string.IsNullOrEmpty(formInfo.AdministratorSmsNotifyMobile))
            //{
            //    var smsPlugin = Context.PluginApi.GetPlugin<SMS.Plugin>();
            //    if (smsPlugin != null && smsPlugin.IsReady)
            //    {
            //        var parameters = new Dictionary<string, string>();
            //        if (!string.IsNullOrEmpty(formInfo.AdministratorSmsNotifyKeys))
            //        {
            //            var keys = formInfo.AdministratorSmsNotifyKeys.Split(',');
            //            foreach (var key in keys)
            //            {
            //                if (FormUtils.EqualsIgnoreCase(key, nameof(DataInfo.Id)))
            //                {
            //                    parameters.Add(key, dataInfo.Id.ToString());
            //                }
            //                else if (FormUtils.EqualsIgnoreCase(key, nameof(DataInfo.AddDate)))
            //                {
            //                    if (dataInfo.AddDate.HasValue)
            //                    {
            //                        parameters.Add(key, dataInfo.AddDate.Value.ToString("yyyy-MM-dd HH:mm"));
            //                    }
            //                }
            //                else
            //                {
            //                    var value = string.Empty;
            //                    var style =
            //                        styleList.FirstOrDefault(x => FormUtils.EqualsIgnoreCase(key, x.Title));
            //                    if (style != null)
            //                    {
            //                        value = LogManager.GetValue(style, dataInfo);
            //                    }

            //                    parameters.Add(key, value);
            //                }
            //            }
            //        }

            //        smsPlugin.Send(formInfo.AdministratorSmsNotifyMobile,
            //            formInfo.AdministratorSmsNotifyTplId, parameters, out _);
            //    }
            //}

            //if (formInfo.IsAdministratorMailNotify &&
            //    !string.IsNullOrEmpty(formInfo.AdministratorMailNotifyAddress))
            //{
            //    var mailPlugin = Context.PluginApi.GetPlugin<Mail.Plugin>();
            //    if (mailPlugin != null && mailPlugin.IsReady)
            //    {
            //        var templateHtml = MailTemplateManager.GetTemplateHtml();
            //        var listHtml = MailTemplateManager.GetListHtml();

            //        var keyValueList = new List<KeyValuePair<string, string>>
            //        {
            //            new KeyValuePair<string, string>("编号", dataInfo.Guid)
            //        };
            //        if (dataInfo.AddDate.HasValue)
            //        {
            //            keyValueList.Add(new KeyValuePair<string, string>("提交时间", dataInfo.AddDate.Value.ToString("yyyy-MM-dd HH:mm")));
            //        }
            //        foreach (var style in styleList)
            //        {
            //            keyValueList.Add(new KeyValuePair<string, string>(style.Title,
            //                LogManager.GetValue(style, dataInfo)));
            //        }

            //        var list = new StringBuilder();
            //        foreach (var kv in keyValueList)
            //        {
            //            list.Append(listHtml.Replace("{{key}}", kv.Key).Replace("{{value}}", kv.Value));
            //        }

            //        var siteInfo = Context.SiteApi.GetSiteInfo(formInfo.SiteId);

            //        mailPlugin.Send(formInfo.AdministratorMailNotifyAddress, string.Empty,
            //            "[SiteServer CMS] 通知邮件",
            //            templateHtml.Replace("{{title}}", $"{formInfo.Title} - {siteInfo.SiteName}").Replace("{{list}}", list.ToString()), out _);
            //    }
            //}

            //if (formInfo.IsUserSmsNotify &&
            //    !string.IsNullOrEmpty(formInfo.UserSmsNotifyTplId) &&
            //    !string.IsNullOrEmpty(formInfo.UserSmsNotifyMobileName))
            //{
            //    var smsPlugin = Context.PluginApi.GetPlugin<SMS.Plugin>();
            //    if (smsPlugin != null && smsPlugin.IsReady)
            //    {
            //        var parameters = new Dictionary<string, string>();
            //        if (!string.IsNullOrEmpty(formInfo.UserSmsNotifyKeys))
            //        {
            //            var keys = formInfo.UserSmsNotifyKeys.Split(',');
            //            foreach (var key in keys)
            //            {
            //                if (FormUtils.EqualsIgnoreCase(key, nameof(DataInfo.Id)))
            //                {
            //                    parameters.Add(key, dataInfo.Id.ToString());
            //                }
            //                else if (FormUtils.EqualsIgnoreCase(key, nameof(DataInfo.AddDate)))
            //                {
            //                    if (dataInfo.AddDate.HasValue)
            //                    {
            //                        parameters.Add(key, dataInfo.AddDate.Value.ToString("yyyy-MM-dd HH:mm"));
            //                    }
            //                }
            //                else
            //                {
            //                    var value = string.Empty;
            //                    var style =
            //                        styleList.FirstOrDefault(x => FormUtils.EqualsIgnoreCase(key, x.Title));
            //                    if (style != null)
            //                    {
            //                        value = LogManager.GetValue(style, dataInfo);
            //                    }

            //                    parameters.Add(key, value);
            //                }
            //            }
            //        }

            //        var mobileFieldInfo = styleList.FirstOrDefault(x => FormUtils.EqualsIgnoreCase(formInfo.UserSmsNotifyMobileName, x.Title));
            //        if (mobileFieldInfo != null)
            //        {
            //            var mobile = LogManager.GetValue(mobileFieldInfo, dataInfo);
            //            if (!string.IsNullOrEmpty(mobile))
            //            {
            //                smsPlugin.Send(mobile, formInfo.UserSmsNotifyTplId, parameters, out _);
            //            }
            //        }
            //    }
            //}
        }

        private string GetTemplatesDirectoryPath()
        {
            return PathUtils.Combine(_plugin.WebRootPath, "assets/form/templates");
        }

        public List<TemplateInfo> GetTemplateInfoList(string type)
        {
            var templateInfoList = new List<TemplateInfo>();

            var directoryPath = GetTemplatesDirectoryPath();
            var directoryNames = DirectoryUtils.GetDirectoryNames(directoryPath);
            foreach (var directoryName in directoryNames)
            {
                var templateInfo = GetTemplateInfo(directoryPath, directoryName);
                if (templateInfo == null) continue;
                if (StringUtils.EqualsIgnoreCase(type, templateInfo.Type))
                {
                    templateInfoList.Add(templateInfo);
                }
            }

            return templateInfoList;
        }

        public TemplateInfo GetTemplateInfo(string name)
        {
            var directoryPath = GetTemplatesDirectoryPath();
            return GetTemplateInfo(directoryPath, name);
        }

        private TemplateInfo GetTemplateInfo(string templatesDirectoryPath, string name)
        {
            TemplateInfo templateInfo = null;

            var configPath = PathUtils.Combine(templatesDirectoryPath, name, "config.json");
            if (FileUtils.IsFileExists(configPath))
            {
                templateInfo = TranslateUtils.JsonDeserialize<TemplateInfo>(FileUtils.ReadText(configPath));
                templateInfo.Name = name;
            }

            return templateInfo;
        }

        public void Clone(string nameToClone, TemplateInfo templateInfo, string templateHtml = null)
        {
            var directoryPath = PathUtils.Combine(_plugin.ContentRootPath, "assets/form/templates");

            DirectoryUtils.Copy(PathUtils.Combine(directoryPath, nameToClone), PathUtils.Combine(directoryPath, templateInfo.Name), true);

            var configJson = TranslateUtils.JsonSerialize(templateInfo);
            var configPath = PathUtils.Combine(directoryPath, templateInfo.Name, "config.json");
            FileUtils.WriteText(configPath, configJson);

            if (templateHtml != null)
            {
                SetTemplateHtml(templateInfo, templateHtml);
            }
        }

        public void Edit(TemplateInfo templateInfo)
        {
            var directoryPath = PathUtils.Combine(_plugin.ContentRootPath, "assets/form/templates");

            var configJson = TranslateUtils.JsonSerialize(templateInfo);
            var configPath = PathUtils.Combine(directoryPath, templateInfo.Name, "config.json");
            FileUtils.WriteText(configPath, configJson);
        }

        public async Task<string> GetTemplateHtmlAsync(TemplateInfo templateInfo)
        {
            var directoryPath = GetTemplatesDirectoryPath();
            var htmlPath = PathUtils.Combine(directoryPath, templateInfo.Name, templateInfo.Main);
            return await _pathManager.GetContentByFilePathAsync(htmlPath);
        }

        public void SetTemplateHtml(TemplateInfo templateInfo, string html)
        {
            var directoryPath = GetTemplatesDirectoryPath();
            var htmlPath = PathUtils.Combine(directoryPath, templateInfo.Name, templateInfo.Main);

            FileUtils.WriteText(htmlPath, html);
        }

        public void DeleteTemplate(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            var directoryPath = GetTemplatesDirectoryPath();
            var templatePath = PathUtils.Combine(directoryPath, name);
            DirectoryUtils.DeleteDirectoryIfExists(templatePath);
        }

        public string GetTableName(FormRequest request)
        {
            return request.FormId > 0 ? FormUtils.TableNameData : FormUtils.TableNameContent;
        }

        public string GetTableName(FormInfo formInfo)
        {
            return formInfo.ChannelId > 0 && formInfo.ContentId > 0 ? FormUtils.TableNameContent : FormUtils.TableNameData;
        }

        public List<int> GetRelatedIdentities(FormRequest request)
        {
            return GetRelatedIdentities(request.SiteId, request.ChannelId, request.ContentId, request.FormId);
        }

        public List<int> GetRelatedIdentities(FormInfo formInfo)
        {
            var formId = formInfo.ChannelId > 0 && formInfo.ContentId > 0 ? 0 : formInfo.Id;
            return GetRelatedIdentities(formInfo.SiteId, formInfo.ChannelId, formInfo.ContentId, formId);
        }

        private static List<int> GetRelatedIdentities(int siteId, int channelId, int contentId, int formId)
        {
            var list = new List<int> { siteId, 0 };
            if (formId > 0)
            {
                list.Insert(0, formId);
            }
            else
            {
                list.Insert(0, channelId);
                list.Insert(0, contentId);
            }

            return list;
        }

        public async Task<List<TableStyle>> GetTableStylesAsync(string tableName, List<int> relatedIdentities)
        {
            return await _tableStyleRepository.GetTableStylesAsync(tableName, relatedIdentities, MetadataAttributes.Value);
        }

        public async Task DeleteTableStyleAsync(string tableName, List<int> relatedIdentities, string attributeName)
        {
            await _tableStyleRepository.DeleteAsync(tableName, relatedIdentities[0], attributeName);
        }

        private static readonly Lazy<List<string>> MetadataAttributes = new Lazy<List<string>>(() => new List<string>
        {
            nameof(DataInfo.FormId),
            nameof(DataInfo.IsReplied),
            nameof(DataInfo.ReplyDate),
            nameof(DataInfo.ReplyContent)
        });

        public int GetPageSize(FormInfo formInfo)
        {
            if (formInfo == null || formInfo.PageSize <= 0) return 30;
            return formInfo.PageSize;
        }
    }
}
