using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.PdfBillingExtensions.Models
{
    public class PdfExtendedSettings : ISettings
    {
        public string BillingCompany { get; set; }
        public string BillingCompanyInfo { get; set; }
        public string BillingCompanyCode { get; set; }
        public string BillingCompanyAddress1 { get; set; }
        public string BillingCompanyAddress2 { get; set; }
        public string BillingSerialNumber { get; set; }
        public string BillingDelegateName { get; set; }
        public string BillingDelegateId { get; set; }
        public int SignaturePictureId { get; set; }
    }
}
