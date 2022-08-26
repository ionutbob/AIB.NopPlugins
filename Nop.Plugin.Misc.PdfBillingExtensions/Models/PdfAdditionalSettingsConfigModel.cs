using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.PdfBillingExtensions.Models
{
    public class PdfAdditionalSettingsConfigModel: BaseNopModel
    {
        public PdfAdditionalSettingsConfigModel()
        {            
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Misc.PdfBillingExtension.BillingCompany")]
        public string BillingCompany { get; set; }
        public bool BillingCompany_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.PdfBillingExtension.BillingCompanyInfo")]
        public string BillingCompanyInfo { get; set; }
        public bool BillingCompanyInfo_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.PdfBillingExtension.BillingCompanyCode")]
        public string BillingCompanyCode { get; set; }
        public bool BillingCompanyCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.PdfBillingExtension.BillingCompanyAddress1")]
        public string BillingCompanyAddress1 { get; set; }
        public bool BillingCompanyAddress1_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.PdfBillingExtension.BillingCompanyAddress2")]
        public string BillingCompanyAddress2 { get; set; }
        public bool BillingCompanyAddress2_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.PdfBillingExtension.BillingSerialNumber")]
        public string BillingSerialNumber { get; set; }
        public bool BillingSerialNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.PdfBillingExtension.BillingDelegateName")]
        public string BillingDelegateName { get; set; }
        public bool BillingDelegateName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.PdfBillingExtension.BillingDelegateId")]
        public string BillingDelegateId { get; set; }
        public bool BillingDelegateId_OverrideForStore { get; set; }


        [NopResourceDisplayName("Plugins.Misc.PdfBillingExtension.SignaturePictureId")]
        [UIHint("Picture")]
        public int SignaturePictureId { get; set; }
        public bool SignaturePictureId_OverrideForStore { get; set; }
    }
}
