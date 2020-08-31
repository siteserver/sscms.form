using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Dto;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Models;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class TemplatesController : ControllerBase
    {
        private const string Route = "form/templates";

        private readonly IAuthManager _authManager;
        private readonly IFormManager _formManager;
        private readonly IFormRepository _formRepository;

        public TemplatesController(IAuthManager authManager, IFormManager formManager, IFormRepository formRepository)
        {
            _authManager = authManager;
            _formManager = formManager;
            _formRepository = formRepository;
        }

        public class ListRequest : SiteRequest
        {
            public string Type { get; set; }
        }

        public class ListResult
        {
            public List<FormInfo> FormInfoList { get; set; }
            public List<TemplateInfo> TemplateInfoList { get; set; }
        }

        public class DeleteRequest : SiteRequest
        {
            public string Type { get; set; }
            public string Name { get; set; }
        }
    }
}
