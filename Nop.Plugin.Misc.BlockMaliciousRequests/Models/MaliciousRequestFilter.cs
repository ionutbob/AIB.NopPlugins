
using System;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Models
{
    public class MaliciousRequestFilter
    {
        public bool IsUrlRequestFilter { get; set; }

        public bool IsCountryFilter { get; set; }

        public bool IsIpFilter { get; set; }

        public string Value { get; set; }

        public DateTime? BlockedUntil { get; set; }
    }
}
