using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Models
{
    public class BlockMaliciousRequestsSettingsConfigModel: BaseNopModel
    {
        public BlockMaliciousRequestsSettingsConfigModel()
        {            
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.EnableRequestUrlFiltering")]
        public bool EnableRequestUrlFiltering { get; set; }
        public bool EnableRequestUrlFiltering_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.EnableCountryFiltering")]
        public bool EnableCountryFiltering { get; set; }
        public bool EnableCountryFiltering_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.EnableIpFiltering")]
        public bool EnableIpFiltering { get; set; }
        public bool EnableIpFiltering_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.BlockBlankIpRequests")]
        public bool BlockBlankIpRequests { get; set; }
        public bool BlockBlankIpRequests_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.EnableDynamicRequestFiltering")]
        public bool EnableDynamicRequestFiltering { get; set; }
        public bool EnableDynamicRequestFiltering_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.MaximumNumberOfRequestsPerMinute")]
        public int MaximumNumberOfRequestsPerMinute { get; set; }
        public int MaximumNumberOfRequestsPerMinute_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.BlockMaliciousRequests.LogAllRequests")]
        public bool LogAllRequests { get; set; }
        public bool LogAllRequests_OverrideForStore { get; set; }
    }
}
