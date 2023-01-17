using SSCMS.Form.Models;
using SSCMS.Models;
using SSCMS.Parse;

namespace SSCMS.Form.Core
{
    public partial class StlForm
    {
        public string ParseInnerHtml(IParseStlContext context, Site site, FormInfo formInfo, string apiUrl)
        {
          return $@"
<script>
var $formConfigApiUrl = '{apiUrl}';
var $formConfigSiteId = {context.SiteId};
var $formConfigChannelId = {context.ChannelId};
var $formConfigContentId = {context.ContentId};
var $formConfigFormId = {formInfo.Id};
</script>
{context.StlInnerHtml}
";
        }
    }
}