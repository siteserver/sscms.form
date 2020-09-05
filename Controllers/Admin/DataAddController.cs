using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Dto;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Core;
using SSCMS.Form.Models;
using SSCMS.Models;
using SSCMS.Services;

namespace SSCMS.Form.Controllers.Admin
{
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class DataAddController : ControllerBase
    {
        private const string Route = "form/dataAdd";
        private const string RouteActionsUpload = "form/{siteId}/dataAdd/actions/upload";

        private readonly IAuthManager _authManager;
        private readonly IPathManager _pathManager;
        private readonly IFormManager _formManager;
        private readonly IDataRepository _dataRepository;
        private readonly IFormRepository _formRepository;

        public DataAddController(IAuthManager authManager, IPathManager pathManager, IFormManager formManager, IDataRepository dataRepository, IFormRepository formRepository)
        {
            _authManager = authManager;
            _pathManager = pathManager;
            _formManager = formManager;
            _dataRepository = dataRepository;
            _formRepository = formRepository;
        }

        public class GetRequest : FormRequest
        {
            public int DataId { get; set; }
        }

        public class GetResult
        {
            public List<TableStyle> Styles { get; set; }
            public DataInfo DataInfo { get; set; }
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
    }
}
