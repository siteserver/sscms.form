using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Configuration;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Core;
using SSCMS.Form.Models;
using SSCMS.Models;
using SSCMS.Repositories;
using SSCMS.Services;

namespace SSCMS.Form.Controllers.Admin
{
    [Authorize(Roles = Types.Roles.Administrator)]
    [Route(Constants.ApiAdminPrefix)]
    public partial class DataController : ControllerBase
    {
        private const string Route = "form/data";
        private const string RouteExport = "form/data/actions/export";
        private const string RouteColumns = "form/data/actions/columns";
        private const string RouteImport = "form/data/actions/import";
        private const string RouteDelete = "form/data/actions/delete";

        private readonly IAuthManager _authManager;
        private readonly IPathManager _pathManager;
        private readonly ISmsManager _smsManager;
        private readonly IFormManager _formManager;
        private readonly IChannelRepository _channelRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IFormRepository _formRepository;
        private readonly IDataRepository _dataRepository;

        public DataController(IAuthManager authManager, IPathManager pathManager, ISmsManager smsManager, IFormManager formManager, IChannelRepository channelRepository, IContentRepository contentRepository, IFormRepository formRepository, IDataRepository dataRepository)
        {
            _authManager = authManager;
            _pathManager = pathManager;
            _smsManager = smsManager;
            _formManager = formManager;
            _channelRepository = channelRepository;
            _contentRepository = contentRepository;
            _formRepository = formRepository;
            _dataRepository = dataRepository;
        }

        public class GetRequest : FormRequest
        {
            public int Page { get; set; }
            public string Keyword { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
        }

        public class GetResult
        {
            public List<DataInfo> Items { get; set; }
            public int Total { get; set; }
            public int PageSize { get; set; }
            public List<TableStyle> Styles { get; set; }
            public List<string> AllAttributeNames { get; set; }
            public List<string> ListAttributeNames { get; set; }
            public bool IsReply { get; set; }
            public List<ContentColumn> Columns { get; set; }
        }

        public class DeleteRequest : FormRequest
        {
            public int Page { get; set; }
            public List<int> DataIds { get; set; }
        }

        public class ColumnsRequest : FormRequest
        {
            public List<string> AttributeNames { get; set; }
        }

        public class ImportRequest
        {
            public int SiteId { get; set; }
            public int FormId { get; set; }
        }
    }
}
