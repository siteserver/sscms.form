using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class DataController
    {
        [HttpPost, Route(RouteDelete)]
        public async Task<ActionResult<DeleteResult>> Delete([FromBody] DeleteRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formRepository.GetFormInfoAsync(request.SiteId, request.FormId);
            if (formInfo == null) return NotFound();

            foreach (var dataId in request.DataIds)
            {
              var dataInfo = await _dataRepository.GetDataInfoAsync(dataId);
              await _dataRepository.DeleteAsync(formInfo, dataInfo);
            }

            var pageSize = _formManager.GetPageSize(formInfo);

            var (total, dataInfoList) = await _dataRepository.GetDataAsync(formInfo, false, null, request.Page, pageSize);
            var items = dataInfoList;

            return new DeleteResult
            {
                Items = items,
                Total = total,
                PageSize = pageSize
            };
        }
    }
}
