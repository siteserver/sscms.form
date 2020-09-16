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

            var formInfo = await _formRepository.GetFormInfoAsync(request.SiteId, request.FormId);
            if (formInfo == null) return NotFound();

            var styles = await _formManager.GetTableStylesAsync(formInfo.Id);

            var dataInfo = await _dataRepository.GetDataInfoAsync(request.DataId);

            var attributeNames = _formRepository.GetAllAttributeNames(styles);
            if (!dataInfo.IsReplied)
            {
                attributeNames.Remove(nameof(DataInfo.ReplyDate));
            }
            attributeNames.Remove(nameof(DataInfo.ReplyContent));

            var allAttributeNames = _formRepository.GetAllAttributeNames(styles);
            var columns = _formManager.GetColumns(allAttributeNames, styles, true);

            return new GetResult
            {
                Columns = columns,
                DataInfo = dataInfo,
                AttributeNames = attributeNames
            };
        }
    }
}
