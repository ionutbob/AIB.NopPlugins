using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Tasks;
using Nop.Plugin.Misc.BlockMaliciousRequests.Data;
using Nop.Plugin.Misc.BlockMaliciousRequests.Domain;
using Nop.Plugin.Misc.BlockMaliciousRequests.Framework;
using Nop.Plugin.Misc.BlockMaliciousRequests.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.BlockMaliciousRequests
{
    public class BlockMaliciousRequestsProcessor : BasePlugin, IMiscPlugin
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly BlockMaliciousRequestsObjectContext _filtersObjectContext;
        private readonly RequestsLogsObjectContext _requestsLogsObjectContext;
        private readonly IRepository<MaliciousRequestFilterRecord> _filtersRepository;
        private readonly IScheduleTaskService _taskSetvice;

        public BlockMaliciousRequestsProcessor(
            ISettingService settingService, 
            ILocalizationService localizationService,
            IWorkContext workContext,
            IWebHelper webHelper,
            BlockMaliciousRequestsObjectContext filtersObjectContext,
            RequestsLogsObjectContext requestsLogsObjectContext,
            IRepository<MaliciousRequestFilterRecord> filtersRepository,
            IScheduleTaskService taskSetvice)
        {
            _settingService = settingService;
            _localizationService = localizationService;            
            _webHelper = webHelper;
            _filtersObjectContext = filtersObjectContext;
            _requestsLogsObjectContext = requestsLogsObjectContext;
            _filtersRepository = filtersRepository;
            _taskSetvice = taskSetvice;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/BlockMaliciousRequests/Configure";
        }

        public override void Install()
        {
            //settings
            _settingService.SaveSetting(
                new BlockMaliciousRequestsSettings() 
                {                     
                    EnableRequestUrlFiltering = true,
                    EnableCountryFiltering = true,
                    MaximumNumberOfRequestsPerMinute = 60, // no user will perform 60 clicks in a minute
                    LogAllRequests = true
                });

            //database objects
            _filtersObjectContext.Install();
            _requestsLogsObjectContext.Install();

            // add locale texts
            // Settings
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.EnableRequestUrlFiltering", "Enable RequestUrl Filtering");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.EnableCountryFiltering", "Enable Country Filtering");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.EnableIpFiltering", "Enable Ip Filtering");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.BlockBlankIpRequests", "Block Blank IP Requests");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.EnableDynamicRequestFiltering", "Enable Dynamic Filtering");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.MaximumNumberOfRequestsPerMinute", "Allowed Requests per Minute");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.LogAllRequests", "Log All Requests");

            // Filters
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.ManageFilters", "Manage Filters");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.UrlFilters", "Url Filters");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.CountryFilters", "Country Filters");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.IpFilters", "IP Filters");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.AddFilter", "Add Filter");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.IsIpFilter", "Is IP Filter");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.IsUrlFilter", "Is Url Filter");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.IsCountryFilter", "Is Country Filter");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.Value", "Value");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.BlockedUntil", "Blocked Until");

            // Logging
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.SeeRequestsLogs", "See Requests Logs");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.RequestsLogs", "Requests Logs");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.Ip", "IP");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.Country", "Country");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.RequestedUrl", "Requested Url");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.RequestTime", "Request Time");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.ResponseStatus", "Response Status");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.RequestDuration", "Request Duration");

            // Add some default url filters
            _filtersRepository.Insert(new MaliciousRequestFilterRecord { IsUrlRequestFilter = true, Value = ".php" });
            _filtersRepository.Insert(new MaliciousRequestFilterRecord { IsUrlRequestFilter = true, Value = "phpunit" });
            _filtersRepository.Insert(new MaliciousRequestFilterRecord { IsUrlRequestFilter = true, Value = "axis2" });
            _filtersRepository.Insert(new MaliciousRequestFilterRecord { IsUrlRequestFilter = true, Value = "jsonws" });
            _filtersRepository.Insert(new MaliciousRequestFilterRecord { IsUrlRequestFilter = true, Value = "cgi-bin" });

            // Add default blocked countries
            _filtersRepository.Insert(new MaliciousRequestFilterRecord { IsCountryFilter = true, Value = "China" });
            _filtersRepository.Insert(new MaliciousRequestFilterRecord { IsCountryFilter = true, Value = "Russia" });
            _filtersRepository.Insert(new MaliciousRequestFilterRecord { IsCountryFilter = true, Value = "Ukraine" });

            // Add task thet deletes log entries
            var deleteLogEntriesTask = new ScheduleTask
            {
                Name = "Delete older requests logs",
                Seconds = 3600,
                StopOnError = false,
                Type = $"{typeof(RemoveLogEntriesTask).FullName}, {typeof(RemoveLogEntriesTask).Assembly.GetName().Name}",
                Enabled = true,
            };
            _taskSetvice.InsertTask(deleteLogEntriesTask);

            // invoke base
            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<BlockMaliciousRequestsSettings>();

            //database objects
            _filtersObjectContext.Uninstall();
            _requestsLogsObjectContext.Uninstall();

            // remove locales
            // Settings
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.EnableRequestUrlFiltering");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.EnableCountryFiltering");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.EnableIpFiltering");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.EnableDynamicRequestFiltering");            
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.MaximumNumberOfRequestsPerMinute");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.BlockBlankIpRequests");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.LogAllRequests");

            // Filters
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.ManageFilters");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.UrlFilters");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.CountryFilters");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.IpFilters");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.AddFilter");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.IsIpFilter");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.IsUrlFilter");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.IsCountryFilter");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.Value");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.BlockedUntil");

            // Logging
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.SeeRequestsLogs");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.RequestsLogs");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.Ip");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.Country");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.RequestedUrl");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.RequestTime");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.ResponseStatus");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.BlockMaliciousRequests.RequestDuration");

            var deleteLogEntriesTask = _taskSetvice.GetAllTasks().FirstOrDefault(t => t.Name == "Delete older requests logs");
            if (deleteLogEntriesTask != null)
            {
                _taskSetvice.DeleteTask(deleteLogEntriesTask);
            }

            // invoke base
            base.Uninstall();
        }
    }
}
