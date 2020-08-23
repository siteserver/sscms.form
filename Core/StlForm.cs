using System.Threading.Tasks;
using System.Web;
using SSCMS.Form.Abstractions;
using SSCMS.Parse;
using SSCMS.Plugins;
using SSCMS.Services;
using SSCMS.Utils;

namespace SSCMS.Form.Core
{
    public class StlForm : IPluginParseAsync
    {
        private const string AttributeTitle = "title";
        private const string AttributeName = "name";
        private const string AttributeType = "type";

        private readonly IPathManager _pathManager;
        private readonly IFormRepository _formRepository;

        public StlForm(IPathManager pathManager, IFormRepository formRepository)
        {
            _pathManager = pathManager;
            _formRepository = formRepository;
        }

        public string ElementName => "stl:form";

        public async Task<string> ParseAsync(IParseStlContext context)
        {
            var formName = string.Empty;
            var type = string.Empty;

            foreach (var name in context.StlAttributes.AllKeys)
            {
                var value = context.StlAttributes[name];

                if (StringUtils.EqualsIgnoreCase(name, AttributeTitle) || StringUtils.EqualsIgnoreCase(name, AttributeName))
                {
                    formName = await context.ParseAsync(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, AttributeType))
                {
                    type = await context.ParseAsync(value);
                }
            }

            var formInfo = !string.IsNullOrEmpty(formName) ? await _formRepository.GetFormInfoByTitleAsync(context.SiteId, formName) : await _formRepository.GetFormInfoByContentIdAsync(context.SiteId, context.ChannelId, context.ContentId);

            if (formInfo == null)
            {
                var formInfoList = await _formRepository.GetFormInfoListAsync(context.SiteId, 0);
                if (formInfoList != null && formInfoList.Count > 0)
                {
                    formInfo = formInfoList[0];
                }
            }

            if (formInfo == null) return string.Empty;

            var apiUrl = _pathManager.GetApiUrl();
            if (string.IsNullOrEmpty(context.StlInnerHtml))
            {
                var elementId = $"iframe_{StringUtils.GetShortGuid(false)}";
                var libUrl = _pathManager.GetRootUrl("assets/form/lib/iframe-resizer-3.6.3/iframeResizer.min.js");
                var pageUrl = _pathManager.GetRootUrl($"templates/{type}/index.html?siteId={context.SiteId}&formId={formInfo.Id}&apiUrl={HttpUtility.UrlEncode(apiUrl)}");

                return $@"
<iframe id=""{elementId}"" frameborder=""0"" scrolling=""no"" src=""{pageUrl}"" style=""width: 1px;min-width: 100%;""></iframe>
<script type=""text/javascript"" src=""{libUrl}""></script>
<script type=""text/javascript"">iFrameResize({{log: false}}, '#{elementId}')</script>
";
            }

            return $@"
<script>
var $config = {{
    apiUrl: '{apiUrl}',
    siteId: {context.SiteId},
    formId: {formInfo.Id}
}};
</script>
{context.StlInnerHtml}
";
        }
    }
}
