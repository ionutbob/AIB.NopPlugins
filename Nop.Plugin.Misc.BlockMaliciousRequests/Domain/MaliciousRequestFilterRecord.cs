using System;
using Nop.Core;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Domain
{
    public class MaliciousRequestFilterRecord : BaseEntity
    {
        public bool IsUrlRequestFilter { get; set; }

        public bool IsCountryFilter { get; set; }

        public bool IsIpFilter { get; set; }

        public string Value { get; set; }

        public DateTime? BlockedUntil { get; set; }
    }
}
