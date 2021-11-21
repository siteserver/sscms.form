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
    public partial class TemplatesLayerEditController : ControllerBase
    {
        private const string Route = "form/templatesLayerEdit";
        private const string RouteUpdate = "form/templatesLayerEdit/actions/update";

        private readonly IAuthManager _authManager;
        private readonly IFormManager _formManager;

        public TemplatesLayerEditController(IAuthManager authManager, IFormManager formManager)
        {
            _authManager = authManager;
            _formManager = formManager;
        }

        public class GetRequest : SiteRequest
        {
            public string Name { get; set; }
        }

        public class GetResult
        {
            public TemplateInfo TemplateInfo { get; set; }
        }

        public class CloneRequest : SiteRequest
        {
            public string Type { get; set; }
            public string OriginalName { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string TemplateHtml { get; set; }
        }

        public class UpdateRequest : SiteRequest
        {
            public string Type { get; set; }
            public string OriginalName { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}
