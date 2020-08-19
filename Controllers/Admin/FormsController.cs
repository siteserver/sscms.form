using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Models;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    [Authorize(Roles = AuthTypes.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class FormsController : ControllerBase
    {
        private const string Route = "form/forms";
        private const string RouteActionsUp = "form/forms/actions/up";
        private const string RouteActionsDown = "form/forms/actions/down";
        private const string RouteExport = "form/forms/actions/export";
        private const string RouteImport = "form/forms/actions/import";

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
