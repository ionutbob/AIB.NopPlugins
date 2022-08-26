using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.IngWebPay.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.IngWebPay.Components
{
    [ViewComponent(Name = "IngWebPay")]
    public class IngWebPayViewComponent : NopViewComponent
    {
        private readonly IngWebPayPaymentSettings _ingWebPayPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public IngWebPayViewComponent(IngWebPayPaymentSettings ingWebPayPaymentSettings,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _ingWebPayPaymentSettings = ingWebPayPaymentSettings;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public IViewComponentResult Invoke()
        {
            var model = new PaymentInfoModel
            {
                DescriptionText = _localizationService.GetLocalizedSetting(_ingWebPayPaymentSettings,
                    x => x.DescriptionText, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id)
            };

            return View("~/Plugins/Payments.IngWebPay/Views/PaymentInfo.cshtml", model);
        }
    }
}