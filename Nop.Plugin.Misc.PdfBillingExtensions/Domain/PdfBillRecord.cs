using System;
using Nop.Core;

namespace Nop.Plugin.Misc.PdfBillingExtensions.Domain
{
    public class PdfBillRecord : BaseEntity
    {
        public int? OrderId { get; set; }

        public int BillNumber { get; set; }

        public DateTime Date { get; set; }
    }
}
