using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Datory;
using SSCMS.Form.Models;
using SSCMS.Form.Utils;
using SSCMS.Form.Utils.Atom.Atom.AdditionalElements;
using SSCMS.Form.Utils.Atom.Atom.AdditionalElements.DublinCore;
using SSCMS.Form.Utils.Atom.Atom.Core;
using SSCMS.Utils;

namespace SSCMS.Form.Core
{
    public partial class FormManager
    {
        private const string VersionFileName = "version.json";

        private static bool IsHistoric(string directoryPath)
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
                if (!StringUtils.EndsWithIgnoreCase(filePath, ".xml")) continue;

                var feed = AtomFeed.Load(new FileStream(filePath, FileMode.Open));

                var formInfo = new FormInfo();

                foreach (var tableColumn in _formRepository.TableColumns)
                {
                    var value = GetValue(feed.AdditionalElements, tableColumn);
                    formInfo.Set(tableColumn.AttributeName, value);
                }

                formInfo.SiteId = siteId;

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

                    foreach (var tableColumn in _dataRepository.TableColumns)
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

                    await _dataRepository.InsertAsync(formInfo, dataInfo);
                }
            }
        }

        private async Task<NameValueCollection> ImportFieldsAsync(int siteId, int formId, string styleDirectoryPath, bool isHistoric)
        {
            var titleAttributeNameDict = new NameValueCollection();

            if (!Directory.Exists(styleDirectoryPath)) return titleAttributeNameDict;

            var formInfo = await _formRepository.GetFormInfoAsync(siteId, formId);
            var relatedIdentities = GetRelatedIdentities(formInfo.Id);
            await _pathManager.ImportStylesByDirectoryAsync(FormUtils.TableNameData, relatedIdentities, styleDirectoryPath);

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

            //var styleDirectoryPath = PathUtils.Combine(directoryPath, formInfo.Id.ToString());

            var relatedIdentities = GetRelatedIdentities(formInfo.Id);

            await _pathManager.ExportStylesAsync(siteId, FormUtils.TableNameData, relatedIdentities);
            //await ExportFieldsAsync(formInfo.Id, styleDirectoryPath);

            var dataInfoList = await _dataRepository.GetAllDataInfoListAsync(formInfo);
            foreach (var dataInfo in dataInfoList)
            {
                var entry = GetAtomEntry(dataInfo);
                feed.Entries.Add(entry);
            }
            feed.Save(filePath);

            var plugin = _pluginManager.GetPlugin(PluginId);

            await FileUtils.WriteTextAsync(PathUtils.Combine(directoryPath, VersionFileName), TranslateUtils.JsonSerialize(new
            {
                CmsVersion = _settingsManager.Version,
                FormVersion = plugin.Version
            }));
        }

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

        private string GetDcElementContent(ScopedElementCollection additionalElements, string name, string defaultContent = "")
        {
            var localName = Prefix + name;
            var element = additionalElements.FindScopedElementByLocalName(localName);
            return element != null ? element.Content : defaultContent;
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

        private string Encrypt(string inputString)
        {
            return TranslateUtils.EncryptStringBySecretKey(inputString, "TgQQk42O");
        }

        private string Decrypt(string inputString)
        {
            return TranslateUtils.DecryptStringBySecretKey(inputString, "TgQQk42O");
        }
    }
}
