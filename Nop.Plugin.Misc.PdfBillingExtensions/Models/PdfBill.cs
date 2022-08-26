using System;

namespace Nop.Plugin.Misc.PdfBillingExtensions.Models
{
    public class PdfBill
    {
        public int OrderId { get; set; }

        public int BillNumber { get; set; }

        public DateTime Date { get; set; }


    }
}
