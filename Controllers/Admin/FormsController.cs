using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Dto;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Models;
using SSCMS.Services;

namespace SSCMS.Form.Controllers.Admin
{
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class FormsController : ControllerBase
    {
        private const string Route = "form/forms";
        private const string RouteUp = "form/forms/actions/up";
        private const string RouteDown = "form/forms/actions/down";
        private const string RouteExport = "form/forms/actions/export";
        private const string RouteImport = "form/forms/actions/import";
        private const string RouteDelete = "form/forms/actions/delete";

        private readonly IAuthManager _authManager;
        private readonly IPathManager _pathManager;
        private readonly IFormManager _formManager;
        private readonly IFormRepository _formRepository;

        public FormsController(IAuthManager authManager, IPathManager pathManager, IFormManager formManager, IFormRepository formRepository)
        {
            _authManager = authManager;
            _pathManager = pathManager;
            _formManager = formManager;
            _formRepository = formRepository;
        }

        public class GetResult
        {
            public List<FormInfo> FormInfoList { get; set; }
        }

        public class DeleteRequest : SiteRequest
        {
            public int FormId { get; set; }
        }

        public class DeleteResult
        {
            public List<FormInfo> FormInfoList { get; set; }
        }

        public class TaxisRequest : SiteRequest
        {
            public int FormId { get; set; }
        }

        public class TaxisResult
        {
            public List<FormInfo> FormInfoList { get; set; }
        }
    }
}
