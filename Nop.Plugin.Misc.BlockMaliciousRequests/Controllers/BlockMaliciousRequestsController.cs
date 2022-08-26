using System.Linq;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Data;
using Nop.Plugin.Misc.BlockMaliciousRequests.Domain;
using Nop.Plugin.Misc.BlockMaliciousRequests.Framework;
using Nop.Plugin.Misc.BlockMaliciousRequests.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class BlockMaliciousRequestsController : BasePaymentController
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IRepository<MaliciousRequestFilterRecord> _filtersRepository;
        private readonly IRepository<RequestLogRecord> _logsRepository;

        #endregion

        #region Ctor

        public BlockMaliciousRequestsController(ILanguageService languageService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IRepository<MaliciousRequestFilterRecord> filtersRepository,
            IRepository<RequestLogRecord> logsRepository)
        {
            _languageService = languageService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _filtersRepository = filtersRepository;
            _logsRepository = logsRepository;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var blockMaliciousRequestsSettings = _settingService.LoadSetting<BlockMaliciousRequestsSettings>(storeScope);

            var model = new BlockMaliciousRequestsSettingsConfigModel
            {
                EnableRequestUrlFiltering = blockMaliciousRequestsSettings.EnableRequestUrlFiltering,
                EnableCountryFiltering = blockMaliciousRequestsSettings.EnableCountryFiltering,
                EnableIpFiltering = blockMaliciousRequestsSettings.EnableIpFiltering,
                BlockBlankIpRequests = blockMaliciousRequestsSettings.BlockBlankIpRequests,
                EnableDynamicRequestFiltering = blockMaliciousRequestsSettings.EnableDynamicRequestFiltering,
                MaximumNumberOfRequestsPerMinute = blockMaliciousRequestsSettings.MaximumNumberOfRequestsPerMinute,
                LogAllRequests = blockMaliciousRequestsSettings.LogAllRequests
            };

            model.ActiveStoreScopeConfiguration = storeScope;

            return View("~/Plugins/Misc.BlockMaliciousRequests/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(BlockMaliciousRequestsSettingsConfigModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var existingSettings = _settingService.LoadSetting<BlockMaliciousRequestsSettings>(storeScope);

            //save settings
            existingSettings.EnableRequestUrlFiltering = model.EnableRequestUrlFiltering;
            existingSettings.EnableCountryFiltering = model.EnableCountryFiltering;
            existingSettings.EnableIpFiltering = model.EnableIpFiltering;
            existingSettings.BlockBlankIpRequests = model.BlockBlankIpRequests;
            existingSettings.EnableDynamicRequestFiltering = model.EnableDynamicRequestFiltering;
            existingSettings.MaximumNumberOfRequestsPerMinute = model.MaximumNumberOfRequestsPerMinute;
            existingSettings.LogAllRequests = model.LogAllRequests;

            _settingService.SaveSetting(existingSettings);
            
            //now clear settings cache
            _settingService.ClearCache();           

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            // Reset middleware in order to load data again
            FilterMaliciousRequestsMiddleware.Reset();

            return Configure();
        }

        #endregion

        #region Filters

        public IActionResult ConfigureFilters()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = new FiltersConfigurationModel();
            model.AvailablePageSizes = "15,20,50";
            model.Length = 15;

            return View("~/Plugins/Misc.BlockMaliciousRequests/Views/ManageFilters.cshtml", model);
        }

        [HttpPost]
        public IActionResult UrlFiltersList(FiltersConfigurationModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            var urlFilters = _filtersRepository.Table.Where(f => f.IsUrlRequestFilter).OrderByDescending(filter => filter.Id);
            var pagedUrlFilters = new PagedList<MaliciousRequestFilterRecord>(urlFilters.AsQueryable(), searchModel.Page - 1, searchModel.PageSize);

            var gridModel = new FiltersListModel().PrepareToGrid(searchModel, pagedUrlFilters, () =>
            {
                return pagedUrlFilters.Select(urlFilter => new MaliciousRequestFilterModel
                {
                    Id = urlFilter.Id,
                    IsUrlRequestFilter = urlFilter.IsUrlRequestFilter,
                    IsCountryFilter = urlFilter.IsCountryFilter,
                    IsIpFilter = urlFilter.IsIpFilter,
                    Value = urlFilter.Value,
                    BlockedUntil = urlFilter.BlockedUntil
                });
            });

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult CountryFiltersList(FiltersConfigurationModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            var countryFilters = _filtersRepository.Table.Where(f => f.IsCountryFilter).OrderByDescending(filter => filter.Id);
            var pagedCountryFilters = new PagedList<MaliciousRequestFilterRecord>(countryFilters.AsQueryable(), searchModel.Page - 1, searchModel.PageSize);

            var gridModel = new FiltersListModel().PrepareToGrid(searchModel, pagedCountryFilters, () =>
            {
                return pagedCountryFilters.Select(urlFilter => new MaliciousRequestFilterModel
                {
                    Id = urlFilter.Id,
                    IsUrlRequestFilter = urlFilter.IsUrlRequestFilter,
                    IsCountryFilter = urlFilter.IsCountryFilter,
                    IsIpFilter = urlFilter.IsIpFilter,
                    Value = urlFilter.Value,
                    BlockedUntil = urlFilter.BlockedUntil
                });
            });

            return Json(gridModel);
        }

        [HttpPost]
        public IActionResult IpFiltersList(FiltersConfigurationModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedDataTablesJson();

            var ipFilters = _filtersRepository.Table.Where(f => f.IsIpFilter).OrderByDescending(filter => filter.Id);
            var pagedIplters = new PagedList<MaliciousRequestFilterRecord>(ipFilters.AsQueryable(), searchModel.Page - 1, searchModel.PageSize);

            var gridModel = new FiltersListModel().PrepareToGrid(searchModel, pagedIplters, () =>
            {
                return pagedIplters.Select(urlFilter => new MaliciousRequestFilterModel
                {
                    Id = urlFilter.Id,
                    IsUrlRequestFilter = urlFilter.IsUrlRequestFilter,
                    IsCountryFilter = urlFilter.IsCountryFilter,
                    IsIpFilter = urlFilter.IsIpFilter,
                    Value = urlFilter.Value,
                    BlockedUntil = urlFilter.BlockedUntil
                });
            });

            return Json(gridModel);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult UpdateUrlFilter(MaliciousRequestFilterModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var filterRecord = _filtersRepository.Table.FirstOrDefault(item => item.Id == model.Id);
            if (filterRecord != null)
            {
                filterRecord.IsUrlRequestFilter = true;
                filterRecord.Value = model.Value;
                filterRecord.BlockedUntil = model.BlockedUntil;

                _filtersRepository.Update(filterRecord);

                FilterMaliciousRequestsMiddleware.Reset();
            }

            return new NullJsonResult();
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult UpdateCountryFilter(MaliciousRequestFilterModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var filterRecord = _filtersRepository.Table.FirstOrDefault(item => item.Id == model.Id);
            if (filterRecord != null)
            {
                filterRecord.IsCountryFilter = true;
                filterRecord.Value = model.Value;
                filterRecord.BlockedUntil = model.BlockedUntil;

                _filtersRepository.Update(filterRecord);

                FilterMaliciousRequestsMiddleware.Reset();
            }

            return new NullJsonResult();
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult UpdateIpFilter(MaliciousRequestFilterModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return Content("Access denied");

            var filterRecord = _filtersRepository.Table.FirstOrDefault(item => item.Id == model.Id);
            if (filterRecord != null)
            {
                filterRecord.IsIpFilter = true;
                filterRecord.Value = model.Value;
                filterRecord.BlockedUntil = model.BlockedUntil;

                _filtersRepository.Update(filterRecord);
            }

            return new NullJsonResult();
        }

        public IActionResult DeleteFilter(MaliciousRequestFilterModel model)
        {
            var recordModel = _filtersRepository.Table.FirstOrDefault(item => item.Id == model.Id);
            if (recordModel != null)
            {
                _filtersRepository.Delete(recordModel);
                FilterMaliciousRequestsMiddleware.Reset();
            }

            return new NullJsonResult();
        }

        public IActionResult AddFilterPopup()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var model = new MaliciousRequestFilterModel();

            return View("/Plugins/Misc.BlockMaliciousRequests/Views/AddFilterPopup.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult AddFilterPopup(MaliciousRequestFilterModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var newFilter = new MaliciousRequestFilterRecord
            {
                IsCountryFilter = model.IsCountryFilter,
                IsIpFilter = model.IsIpFilter,
                IsUrlRequestFilter = model.IsUrlRequestFilter,
                Value = model.Value,
                BlockedUntil = model.BlockedUntil
            };

            _filtersRepository.Insert(newFilter);

            FilterMaliciousRequestsMiddleware.Reset();

            ViewBag.RefreshPage = true;

            return View("~/Plugins/Misc.BlockMaliciousRequests/Views/AddFilterPopup.cshtml", model);
        }

        #endregion

        #region Requests Logs

        public IActionResult SeeLogs()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var model = new LogsConfigurationModel();
            model.AvailablePageSizes = "20,50,100";
            model.Length = 100;     

            return View("~/Plugins/Misc.BlockMaliciousRequests/Views/RequestsLogs.cshtml", model);
        }

        public IActionResult GetLogs(LogsConfigurationModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var logRecords = _logsRepository.Table.OrderByDescending(l => l.RequestTime);

            var pagedLogRecords = new PagedList<RequestLogRecord>(logRecords.AsQueryable(), searchModel.Page - 1, searchModel.PageSize);

            var gridModel = new LogsListModel().PrepareToGrid(searchModel, pagedLogRecords, () =>
            {
                return pagedLogRecords.Select(lr => new RequestLogModel
                {
                    Id = lr.Id,
                    BlockedBy = lr.BlockedBy,
                    Country = lr.Country,
                    Ip = lr.Ip,
                    RequestedUrl = lr.RequestedUrl,
                    ResponseStatus = lr.ResponseStatus,
                    RequestTime = lr.RequestTime,
                    RequestDuration = lr.RequestDuration
                });
            });

            return Json(gridModel);
        }

        #endregion
    }
}