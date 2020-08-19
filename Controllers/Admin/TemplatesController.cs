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
    public partial class TemplatesController : ControllerBase
    {
        private const string Route = "form/templates";

        private readonly IAuthManager _authManager;
        private readonly IFormManager _formManager;

        public TemplatesController(IAuthManager authManager, IFormManager formManager)
        {
            _authManager = authManager;
            _formManager = formManager;
        }

        public class ListRequest : SiteRequest
        {
            public string Type { get; set; }
        }

        public class ListResult
        {
            public List<TemplateInfo> TemplateInfoList { get; set; }
        }

        public class DeleteRequest : SiteRequest
        {
            public string Type { get; set; }
            public string Name { get; set; }
        }
    }
}
