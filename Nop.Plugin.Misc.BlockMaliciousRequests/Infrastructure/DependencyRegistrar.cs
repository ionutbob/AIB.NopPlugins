using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Misc.BlockMaliciousRequests.Data;
using Nop.Plugin.Misc.BlockMaliciousRequests.Domain;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 1;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            //data context
            builder.RegisterPluginDataContext<BlockMaliciousRequestsObjectContext>("nop_object_context_block_malicious_requests");
            builder.RegisterPluginDataContext<RequestsLogsObjectContext>("nop_object_context_requests_logs");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<MaliciousRequestFilterRecord>>().As<IRepository<MaliciousRequestFilterRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_block_malicious_requests"))
                .InstancePerLifetimeScope();            
            builder.RegisterType<EfRepository<RequestLogRecord>>().As<IRepository<RequestLogRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_requests_logs"))
                .InstancePerLifetimeScope();
        }
    }
}
