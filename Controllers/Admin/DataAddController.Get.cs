using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class DataAddController
    {
        [HttpGet, Route(Route)]
        public async Task<ActionResult<GetResult>> Get([FromQuery] GetRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

            var formInfo = await _formManager.GetFormInfoByRequestAsync(request.SiteId, request.ChannelId,
                request.ContentId, request.FormId);
            if (formInfo == null) return NotFound();

            var styles = await _formManager.GetTableStylesAsync(formInfo.Id);
            //var value = new Dictionary<string, object>();
            //if (request.DataId > 0)
            //{
            //    var dataInfo = await _dataRepository.GetDataInfoAsync(request.DataId);
            //    value = _dataRepository.GetDict(styles, dataInfo);
            //}
            var dataInfo = await _formManager.GetDataInfoAsync(request.DataId, formInfo.Id, styles);

            //foreach (var style in styles)
            //{
            //    object value;
            //    if (style.InputType == InputType.CheckBox || style.InputType == InputType.SelectMultiple)
            //    {
            //        value = dataInfo != null
            //            ? TranslateUtils.JsonDeserialize<List<string>>(dataInfo.Get<string>(style.AttributeName))
            //            : new List<string>();
            //    }
            //    //else if (style.FieldType == InputType.Image.Value)
            //    //{
            //    //    value = dataInfo != null
            //    //        ? new List<string> {dataInfo.Get<string>(style.Title)}
            //    //        : new List<string>();
            //    //}
            //    else if (style.InputType == InputType.Date || style.InputType == InputType.DateTime)
            //    {
            //        value = dataInfo?.Get<DateTime>(style.AttributeName);
            //    }
            //    else
            //    {
            //        value = dataInfo?.Get<string>(style.AttributeName);
            //    }

            //    if (value == null)
            //    {
            //        value = string.Empty;
            //    }
            //}

            return new GetResult
            {
                Styles = styles,
                DataInfo = dataInfo
            };
        }
    }
}