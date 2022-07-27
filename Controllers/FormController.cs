using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Models;
using SSCMS.Models;
using SSCMS.Services;

namespace SSCMS.Form.Controllers
{
    [Route("api/form")]
    public partial class FormController : ControllerBase
    {
        private readonly ICacheManager _cacheManager;
        private readonly ISmsManager _smsManager;
        private readonly IFormManager _formManager;
        private readonly IFormRepository _formRepository;
        private readonly IDataRepository _dataRepository;

        public FormController(ICacheManager cacheManager, ISmsManager smsManager, IFormManager formManager, IFormRepository formRepository, IDataRepository dataRepository)
        {
            _cacheManager = cacheManager;
            _smsManager = smsManager;
            _formManager = formManager;
            _formRepository = formRepository;
            _dataRepository = dataRepository;
        }

        public class GetFormResult
        {
            public List<TableStyle> Styles { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string SuccessMessage { get; set; }
            public string SuccessCallback { get; set; }
            public bool IsSms { get; set; }
            public bool IsCaptcha { get; set; }
            public DataInfo DataInfo { get; set; }
        }

        public class ListRequest
        {
            public int Page { get; set; }
            public string Word { get; set; }
        }

        public class ListResult
        {
            public List<DataInfo> Items { get; set; }
            public int Total { get; set; }
            public int PageSize { get; set; }
            public List<TableStyle> Styles { get; set; }
            public List<string> AllAttributeNames { get; set; }
            public List<string> ListAttributeNames { get; set; }
            public bool IsReply { get; set; }
            public List<ContentColumn> Columns { get; set; }
        }

        public class SendSmsRequest
        {
            public string Mobile { get; set; }
        }

        private static string GetUploadTokenCacheKey(int formId)
        {
            return $"SSCMS.Form.Controllers.Actions.Upload.{formId}";
        }

        private string GetSmsCodeCacheKey(int formId, string mobile)
        {
            return $"SSCMS.Form.Controllers.Actions.SendSms.{formId}.{mobile}";
        }
    }
}
