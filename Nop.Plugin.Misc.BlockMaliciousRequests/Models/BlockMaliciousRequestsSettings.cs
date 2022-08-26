using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Models
{
    public class BlockMaliciousRequestsSettings : ISettings
    {
        public bool EnableRequestUrlFiltering { get; set; }
        public bool EnableCountryFiltering { get; set; }
        public bool EnableIpFiltering { get; set; }
        public bool BlockBlankIpRequests { get; set; }
        public bool EnableDynamicRequestFiltering { get; set; }
        public int MaximumNumberOfRequestsPerMinute { get; set; }
        public bool LogAllRequests { get; set; }
    }
}
