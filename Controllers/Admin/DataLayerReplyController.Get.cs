using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Core;
using SSCMS.Form.Models;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class DataLayerReplyController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] GetRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formManager.GetFormInfoByRequestAsync(request.SiteId, request.ChannelId, request.ContentId, request.FormId);
            if (formInfo == null) return NotFound();

            var tableName = _formManager.GetTableName(request);
            var relatedIdentities = _formManager.GetRelatedIdentities(request);
            var styles = await _formManager.GetTableStylesAsync(tableName, relatedIdentities);

            var dataInfo = await _dataRepository.GetDataInfoAsync(request.DataId);

            var attributeNames = _formRepository.GetAllAttributeNames(styles);
            if (!dataInfo.IsReplied)
            {
                attributeNames.Remove(nameof(DataInfo.ReplyDate));
            }
            attributeNames.Remove(nameof(DataInfo.ReplyContent));

            return new GetResult
            {
                Dict = dataInfo.ToDictionary(),
                AttributeNames = attributeNames
            };
        }
    }
}
