using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Utils;

namespace SSCMS.Form.Controllers
{
    public partial class FormController
    {
        [HttpGet, Route("{siteId:int}/{formId:int}")]
        public async Task<ActionResult<ListResult>> List([FromRoute] int siteId, [FromRoute] int formId, [FromQuery] ListRequest request)
        {
            var formInfo = await _formRepository.GetFormInfoAsync(siteId, formId);
            if (formInfo == null) return NotFound();

            var styles = await _formManager.GetTableStylesAsync(formInfo.Id);

            var listAttributeNames = ListUtils.GetStringList(formInfo.ListAttributeNames);
            var allAttributeNames = _formRepository.GetAllAttributeNames(styles);
            var pageSize = _formManager.GetPageSize(formInfo);

            var (total, dataInfoList) = await _dataRepository.GetDataAsync(formInfo, formInfo.IsReply, request.Word, request.Page, pageSize);
            var items = dataInfoList;

            var columns = _formManager.GetColumns(listAttributeNames, styles, formInfo.IsReply);

            return new ListResult
            {
                Items = items,
                Total = total,
                PageSize = pageSize,
                Styles = styles,
                AllAttributeNames = allAttributeNames,
                ListAttributeNames = listAttributeNames,
                IsReply = formInfo.IsReply,
                Columns = columns
            };
        }
    }
}
