using Microsoft.AspNetCore.Builder;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Framework
{
    static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseFilterMaliciousRequestsMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FilterMaliciousRequestsMiddleware>();
        }
    }
}
