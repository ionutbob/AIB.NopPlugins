using System;
using Nop.Core;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Domain
{
    public class RequestLogRecord : BaseEntity
    {
        public string Ip { get; set; }

        public string Country { get; set; }

        public string RequestedUrl { get; set; }

        public DateTime RequestTime { get; set; }

        public int ResponseStatus { get; set; }

        public BlockedBy BlockedBy { get; set; }

        public TimeSpan RequestDuration { get; set; }
    }
    public enum BlockedBy
    {
        None = 0,
        Url = 1,
        Ip = 2,
        Country = 3,
        Dynamically = 4
    }
}
