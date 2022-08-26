using System;
using Nop.Plugin.Misc.BlockMaliciousRequests.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Models
{
    public class RequestLogModel : BaseNopModel
    {
        public int Id { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.Ip")]
        public string Ip { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.Country")]
        public string Country { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.RequestedUrl")]
        public string RequestedUrl { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.RequestTime")]
        public DateTime RequestTime { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.ResponseStatus")]
        public int ResponseStatus { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.IsUrlFilter")]
        public BlockedBy BlockedBy { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.RequestDuration")]
        public TimeSpan RequestDuration { get; set; }
    }
}
