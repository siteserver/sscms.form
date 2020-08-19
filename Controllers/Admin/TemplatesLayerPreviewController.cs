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
    public partial class TemplatesLayerPreviewController : ControllerBase
    {
        private const string Route = "form/templatesLayerPreview";

        private readonly IAuthManager _authManager;
        private readonly IFormManager _formManager;
        private readonly IFormRepository _formRepository;

        public TemplatesLayerPreviewController(IAuthManager authManager, IFormManager formManager, IFormRepository formRepository)
        {
            _authManager = authManager;
            _formManager = formManager;
            _formRepository = formRepository;
        }

        public class GetRequest : SiteRequest
        {
            public string Type { get; set; }
            public string Name { get; set; }
        }

        public class GetResult
        {
            public TemplateInfo TemplateInfo { get; set; }
            public List<FormInfo> FormInfoList { get; set; }
        }
    }
}
