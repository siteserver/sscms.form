using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Dto;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class DataLayerReplyController
    {
        [HttpPost, Route(Route)]
        public async Task<ActionResult<BoolResult>> Submit([FromBody] SubmitRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formRepository.GetFormInfoAsync(request.SiteId, request.FormId);
            if (formInfo == null) return NotFound();

            var dataInfo = await _dataRepository.GetDataInfoAsync(request.DataId);
            if (dataInfo == null) return NotFound();

            dataInfo.ReplyContent = request.ReplyContent;

            await _dataRepository.ReplyAsync(formInfo, dataInfo);

            return new BoolResult
            {
                Value = true
            };
        }
    }
}
