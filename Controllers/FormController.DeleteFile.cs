using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SSCMS.Form.Controllers
{
    public partial class FormController
    {
        [HttpDelete, Route("{siteId:int}/{formId:int}/actions/upload")]
        public async Task<ActionResult<DeleteResult>> DeleteFile([FromRoute]int siteId, [FromRoute] int formId, [FromBody] DeleteRequest request)
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

            return new DeleteResult
            {
                FieldId = request.FieldId
            };
        }

        public class DeleteRequest
        {
            public int FieldId { get; set; }
            public string UploadToken { get; set; }
        }

        public class DeleteResult
        {
            public int FieldId { get; set; }
        }
    }
}
