using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.PdfBillingExtensions.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.PdfBillingExtensions.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class PdfExtendedSettingsController : BasePaymentController
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public PdfExtendedSettingsController(ILanguageService languageService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _languageService = languageService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var pdfExtendedSettings = _settingService.LoadSetting<PdfExtendedSettings>(storeScope);

            var model = new PdfAdditionalSettingsConfigModel
            {
                BillingCompany = pdfExtendedSettings.BillingCompany,
                BillingCompanyAddress1 = pdfExtendedSettings.BillingCompanyAddress1,
                BillingCompanyAddress2 = pdfExtendedSettings.BillingCompanyAddress2,
                BillingCompanyCode = pdfExtendedSettings.BillingCompanyCode,
                BillingCompanyInfo = pdfExtendedSettings.BillingCompanyInfo,
                BillingDelegateName = pdfExtendedSettings.BillingDelegateName,
                BillingSerialNumber = pdfExtendedSettings.BillingSerialNumber,
                SignaturePictureId = pdfExtendedSettings.SignaturePictureId,
                BillingDelegateId = pdfExtendedSettings.BillingDelegateId
            };

            model.ActiveStoreScopeConfiguration = storeScope;

            return View("~/Plugins/Misc.PdfBillingExtensions/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(PdfAdditionalSettingsConfigModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var existingSettings = _settingService.LoadSetting<PdfExtendedSettings>(storeScope);

            //save settings
            existingSettings.BillingCompany = model.BillingCompany;
            existingSettings.BillingCompanyAddress1 = model.BillingCompanyAddress1;
            existingSettings.BillingCompanyAddress2 = model.BillingCompanyAddress2;
            existingSettings.BillingCompanyCode = model.BillingCompanyCode;
            existingSettings.BillingCompanyInfo = model.BillingCompanyInfo;
            existingSettings.BillingSerialNumber = model.BillingSerialNumber;
            existingSettings.BillingDelegateName = model.BillingDelegateName;
            existingSettings.SignaturePictureId = model.SignaturePictureId;
            existingSettings.BillingDelegateId = model.BillingDelegateId;

            _settingService.SaveSetting(existingSettings);
            
            //now clear settings cache
            _settingService.ClearCache();           

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion
    }
}