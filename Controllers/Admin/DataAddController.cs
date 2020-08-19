using System.Collections.Generic;
using Datory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Core;
using SSCMS.Models;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    [Authorize(Roles = AuthTypes.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class DataAddController : ControllerBase
    {
        private const string Route = "form/dataAdd";
        private const string RouteActionsUpload = "form/dataAdd/actions/upload";

        private readonly IAuthManager _authManager;
        private readonly IPathManager _pathManager;
        private readonly IFormManager _formManager;
        private readonly IDataRepository _dataRepository;

        public DataAddController(IAuthManager authManager, IPathManager pathManager, IFormManager formManager, IDataRepository dataRepository)
        {
            _authManager = authManager;
            _pathManager = pathManager;
            _formManager = formManager;
            _dataRepository = dataRepository;
        }

        public class GetRequest : FormRequest
        {
            public int DataId { get; set; }
        }

        public class GetResult
        {
            public List<TableStyle> Styles { get; set; }
            public Dictionary<string, object> Value { get; set; }
        }

        public class UploadRequest : SiteRequest
        {
            public int FieldId { get; set; }
        }

        public class UploadResult
        {
            public string ImageUrl { get; set; }
            public int FieldId { get; set; }
        }

        public class DeleteRequest : SiteRequest
        {
            public string FileUrl { get; set; }
        }

        public class SubmitRequest : Entity
        {
            public int SiteId { get; set; }
            public int ChannelId { get; set; }
            public int ContentId { get; set; }
            public int FormId { get; set; }
            public int DataId { get; set; }
        }
    }
}
