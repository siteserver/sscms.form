﻿using Microsoft.AspNetCore.Authorization;
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
    public partial class FormsLayerAddController : ControllerBase
    {
        private const string Route = "form/formsLayerAdd";
        private const string RouteUpdate = "form/formsLayerAdd/actions/update";

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

        public class UpdateRequest : SiteRequest
        {
            public int FormId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
        }
    }
}
