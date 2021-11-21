using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSCMS.Form.Core;

namespace SSCMS.Form.Controllers.Admin
{
    public partial class DataAddController
    {
        [HttpPost, Route(RouteUpload)]
        public async Task<ActionResult<UploadResult>> UploadFile([FromBody] UploadRequest request)
        {
            if (!await _authManager.HasSitePermissionsAsync(request.SiteId, FormManager.PermissionsForms))
                return Unauthorized();

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
    }
}
