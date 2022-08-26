using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.IngWebPay.Models
{
    public class PaymentInfoModel : BaseNopModel, ILocalizedModel<PaymentInfoModel.PaymentInfoLocalizedModel>
    {
        public string DescriptionText { get; set; }
        public IList<PaymentInfoLocalizedModel> Locales { get; set; }

        #region Nested class

        public partial class PaymentInfoLocalizedModel : ILocalizedLocaleModel
        {
            public int LanguageId { get; set; }

            [NopResourceDisplayName("Plugins.Payment.IngWebPay.DescriptionText")]
            public string DescriptionText { get; set; }
        }

        #endregion

    }
}