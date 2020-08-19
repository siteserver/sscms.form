using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SSCMS.Form.Controllers
{
    public partial class FormController
    {
        [HttpPost, Route("{siteId:int}/{formId:int}/actions/upload")]
        public async Task<ActionResult<UploadResult>> UploadFile([FromRoute] int siteId, [FromRoute] int formId, [FromBody] UploadRequest request)
        {
            var cacheKey = GetUploadTokenCacheKey(formId);
            var cacheList = _cacheManager.Get(cacheKey) ?? new List<string>();
            if (!cacheList.Contains(request.UploadToken))
            {
                return Unauthorized();
            }

            //TODO
            //var style = await _fieldRepository.GetFieldInfoAsync(formId, request.FieldId);
            //if (style.InputType != InputType.Image)
            //{
            //    return Unauthorized();
            //}

            var imageUrl = string.Empty;

            //TODO
            //foreach (string name in HttpContext.Current.Request.Files)
            //{
            //    var postFile = HttpContext.Current.Request.Files[name];

            //    if (postFile == null)
            //    {
            //        return this.Error("Could not read image from body");
            //    }

            //    var filePath = Context.SiteApi.GetUploadFilePath(siteId, postFile.FileName);

            //    if (!FormUtils.IsImage(Path.GetExtension(filePath)))
            //    {
            //        return this.Error("image file extension is not correct");
            //    }

            //    postFile.SaveAs(filePath);

            //    imageUrl = Context.SiteApi.GetSiteUrlByFilePath(filePath);
            //}

            return new UploadResult
            {
                ImageUrl = imageUrl,
                FieldId = request.FieldId
            };
        }

        public class UploadRequest
        {
            public int FieldId { get; set; }
            public string UploadToken { get; set; }
        }

        public class UploadResult
        {
            public string ImageUrl { get; set; }
            public int FieldId { get; set; }
        }
    }
}
