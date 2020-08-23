using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Dto;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Core;
using SSCMS.Models;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers.Admin
{
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class StylesController : ControllerBase
    {
        private const string Route = "form/styles";
        private const string RouteImport = "form/styles/actions/import";
        private const string RouteExport = "form/styles/actions/export";

        private readonly IAuthManager _authManager;
        private readonly IPathManager _pathManager;
        private readonly IFormManager _formManager;

        public StylesController(IAuthManager authManager, IPathManager pathManager, IFormManager formManager)
        {
            _authManager = authManager;
            _pathManager = pathManager;
            _formManager = formManager;
        }

        public class GetResult
        {
            public IEnumerable<Select<string>> InputTypes { get; set; }
            public string TableName { get; set; }
            public string RelatedIdentities { get; set; }
            public List<TableStyle> Styles { get; set; }
        }

        public class DeleteRequest : FormRequest
        {
            public string AttributeName { get; set; }
        }

        public class DeleteResult
        {
            public List<TableStyle> Styles { get; set; }
        }
    }
}
