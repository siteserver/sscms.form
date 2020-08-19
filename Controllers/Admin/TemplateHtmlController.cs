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
    public partial class TemplateHtmlController : ControllerBase
    {
        private const string Route = "form/templateHtml";

        private readonly IAuthManager _authManager;
        private readonly IFormManager _formManager;

        public TemplateHtmlController(IAuthManager authManager, IFormManager formManager)
        {
            _authManager = authManager;
            _formManager = formManager;
        }

        public class GetRequest : SiteRequest
        {
            public string Type { get; set; }
            public string Name { get; set; }
        }

        public class GetResult
        {
            public TemplateInfo TemplateInfo { get; set; }
            public string TemplateHtml { get; set; }
            public bool IsSystem { get; set; }
        }

        public class SubmitRequest : SiteRequest
        {
            public string Name { get; set; }
            public string TemplateHtml { get; set; }
        }
    }
}
