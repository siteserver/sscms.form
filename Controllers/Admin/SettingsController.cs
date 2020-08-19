using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Core;
using SSCMS.Form.Models;
using SSCMS.Models;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    [Authorize(Roles = AuthTypes.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class SettingsController : ControllerBase
    {
        private const string Route = "form/settings";

        private readonly IAuthManager _authManager;
        private readonly IFormManager _formManager;
        private readonly IFormRepository _formRepository;

        public SettingsController(IAuthManager authManager, IFormManager formManager, IFormRepository formRepository)
        {
            _authManager = authManager;
            _formManager = formManager;
            _formRepository = formRepository;
        }

        public class GetResult
        {
            public FormInfo Form { get; set; }
            public List<TableStyle> Styles { get; set; }
            public List<string> AttributeNames { get; set; }
            public List<string> AdministratorSmsNotifyKeys { get; set; }
            public List<string> UserSmsNotifyKeys { get; set; }
        }

        public class SubmitRequest : FormRequest
        {
            public FormInfo Form { get; set; }
        }
    }
}
