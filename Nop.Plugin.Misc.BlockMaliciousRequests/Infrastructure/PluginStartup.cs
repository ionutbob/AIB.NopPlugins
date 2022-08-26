using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.BlockMaliciousRequests.Data;
using Nop.Plugin.Misc.BlockMaliciousRequests.Framework;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Infrastructure
{
    public class PluginStartup : INopStartup
    {
        /// <summary>
        /// This middleware should be added first
        /// </summary>
        public int Order => -1000;

        public void Configure(IApplicationBuilder application)
        {
            application.UseFilterMaliciousRequestsMiddleware();
        }

        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //add object context
            services.AddDbContext<BlockMaliciousRequestsObjectContext>(optionsBuilder =>
            {
                optionsBuilder.UseSqlServerWithLazyLoading(services);
            });

            services.AddDbContext<RequestsLogsObjectContext>(optionsBuilder =>
            {
                optionsBuilder.UseSqlServerWithLazyLoading(services);
            });
        }
    }
}
