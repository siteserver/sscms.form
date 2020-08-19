using Microsoft.Extensions.DependencyInjection;
using SSCMS.Form.Abstractions;
using SSCMS.Form.Core;
using SSCMS.Plugins;

namespace SSCMS.Form
{
    public class Startup : IPluginConfigureServices
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IFormRepository, FormRepository>();
            services.AddScoped<IDataRepository, DataRepository>();
            services.AddScoped<IFormManager, FormManager>();
        }
    }
}
