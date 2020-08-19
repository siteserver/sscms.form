using SSCMS.Dto;

namespace SSCMS.Form.Core
{
    public class FormRequest : SiteRequest
    {
        public int ChannelId { get; set; }
        public int ContentId { get; set; }
        public int FormId { get; set; }
    }
}
