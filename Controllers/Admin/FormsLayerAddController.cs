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
    public partial class FormsLayerAddController : ControllerBase
    {
        private const string Route = "form/formsLayerAdd";

        private readonly IAuthManager _authManager;
        private readonly IFormManager _formManager;
        private readonly IFormRepository _formRepository;

        public FormsLayerAddController(IAuthManager authManager, IFormManager formManager, IFormRepository formRepository)
        {
            _authManager = authManager;
            _formManager = formManager;
            _formRepository = formRepository;
        }

        public class GetRequest : SiteRequest
        {
            public int FormId { get; set; }
        }

        public class GetResult
        {
            public FormInfo FormInfo { get; set; }
        }

        public class AddRequest : SiteRequest
        {
            public string Title { get; set; }
            public string Description { get; set; }
        }

        public class EditRequest : SiteRequest
        {
            public int FormId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
        }
    }
}
