using System.Threading.Tasks;
using SSCMS.Form.Abstractions;
using SSCMS.Plugins;

namespace SSCMS.Form.Core
{
    public class ContentHandler : PluginContentHandler
    {
        private readonly IFormManager _formManager;

        public ContentHandler(IFormManager formManager)
        {
            _formManager = formManager;
        }

        //public override async Task OnTranslatedAsync(int siteId, int channelId, int contentId, int targetSiteId, int targetChannelId, int targetContentId)
        //{
        //    await _formManager.TranslateAsync(siteId, channelId, contentId, targetSiteId, targetChannelId,
        //        targetContentId);
        //}
    }
}
