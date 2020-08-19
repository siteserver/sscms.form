using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Abstractions;
using SSCMS.Services;

namespace SSCMS.Form.Controllers
{
    [Route("api/form")]
    public partial class FormController : ControllerBase
    {
        private readonly ICacheManager<List<string>> _cacheManager;
        private readonly IFormManager _formManager;
        private readonly IFormRepository _formRepository;
        private readonly IDataRepository _dataRepository;

        public FormController(ICacheManager<List<string>> cacheManager, IFormManager formManager, IFormRepository formRepository, IDataRepository dataRepository)
        {
            _cacheManager = cacheManager;
            _formManager = formManager;
            _formRepository = formRepository;
            _dataRepository = dataRepository;
        }

        private static string GetUploadTokenCacheKey(int formId)
        {
            return $"SSCMS.Form.Controllers.Actions.Upload.{formId}";
        }
    }
}
