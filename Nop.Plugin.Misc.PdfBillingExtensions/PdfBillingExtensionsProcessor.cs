using Nop.Core;
using Nop.Plugin.Misc.PdfBillingExtensions.Data;
using Nop.Plugin.Misc.PdfBillingExtensions.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.PdfBillingExtensions
{
    public class PdfBillingExtensionsProcessor : BasePlugin, IMiscPlugin
    {
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly PdfBillingExtensionsObjectContext _objectContext;

        public PdfBillingExtensionsProcessor(
            ISettingService settingService, 
            ILocalizationService localizationService,
            IWebHelper webHelper,
            PdfBillingExtensionsObjectContext objectContext)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _objectContext = objectContext;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PdfExtendedSettings/Configure";
        }

        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new PdfExtendedSettings());

            //database objects
            _objectContext.Install();

            // add locale texts
            //
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.PdfBillingExtension.Bill", "Bill");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.PdfBillingExtension.SellerInfo", "Seller Info");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.PdfBillingExtension.ClientInfo", "Client Info");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingCompany", "Company");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingCompanyInfo", "Register Info");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingCompanyCode", "Tax Code");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingCompanyAddress1", "Address 1");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingCompanyAddress2", "Address 2");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingSerialNumber", "Serial");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingNumber", "No");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingDelegateName", "Made by");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingDelegateId", "Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.PdfBillingExtension.SignaturePictureId", "Signature image");

            // invoke base
            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<PdfExtendedSettings>();

            //database objects
            _objectContext.Uninstall();

            // remove locales
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.PdfBillingExtension.Bill");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.PdfBillingExtension.SellerInfo");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.PdfBillingExtension.ClientInfo");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingCompany");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingCompanyInfo");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingCompanyCode");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingCompanyAddress1");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingCompanyAddress2");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingSerialNumber");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingDelegateName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.PdfBillingExtension.BillingDelegateId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.PdfBillingExtension.SignaturePictureId");            

            // invoke base
            base.Uninstall();
        }
    }
}
