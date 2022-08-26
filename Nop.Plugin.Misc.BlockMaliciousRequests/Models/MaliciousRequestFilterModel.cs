using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Models
{
    public class MaliciousRequestFilterModel : BaseNopModel
    {
        public int Id { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.IsUrlFilter")]
        public bool IsUrlRequestFilter { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.IsCountryFilter")]
        public bool IsCountryFilter { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.IsIpFilter")]
        public bool IsIpFilter { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.Value")]
        public string Value { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.BlockedUntil")]
        [UIHint("DateNullable")]
        public DateTime? BlockedUntil { get; set; }
    }
}
