using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Core;
using SSCMS.Services;

namespace SSCMS.Form.Controllers.Admin
{
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class DataLayerReplyController : ControllerBase
    {
        private const string Route = "form/dataLayerReply";

        private readonly IAuthManager _authManager;
        private readonly IFormManager _formManager;
        private readonly IFormRepository _formRepository;
        private readonly IDataRepository _dataRepository;

        public DataLayerReplyController(IAuthManager authManager, IFormManager formManager, IFormRepository formRepository, IDataRepository dataRepository)
        {
            _authManager = authManager;
            _formManager = formManager;
            _formRepository = formRepository;
            _dataRepository = dataRepository;
        }

        public class GetRequest : FormRequest
        {
            public int DataId { get; set; }
        }

        public class GetResult
        {
            public IDictionary<string, object> Dict { get; set; }
            public List<string> AttributeNames { get; set; }
        }

        public class SubmitRequest : GetRequest
        {
            public string ReplyContent { get; set; }
        }
    }
}
