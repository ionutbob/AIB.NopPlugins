using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Misc.PdfBillingExtensions.Data;
using Nop.Plugin.Misc.PdfBillingExtensions.Domain;
using Nop.Plugin.Misc.PdfBillingExtensions.Services;
using Nop.Services.Common;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Misc.PdfBillingExtensions.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 1;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            // Register billing service
            builder.RegisterType<PdfBillService>().As<IPdfService>().InstancePerLifetimeScope();

            //data context
            builder.RegisterPluginDataContext<PdfBillingExtensionsObjectContext>("nop_object_context_billing_extensions");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<PdfBillRecord>>().As<IRepository<PdfBillRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_billing_extensions"))
                .InstancePerLifetimeScope();
        }
    }
}
