using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.PdfBillingExtensions.Domain;

namespace Nop.Plugin.Misc.PdfBillingExtensions.Data
{
    public partial class PdfBillRecordMap : NopEntityTypeConfiguration<PdfBillRecord>
    {
        public override void Configure(EntityTypeBuilder<PdfBillRecord> builder)
        {
            builder.ToTable(nameof(PdfBillRecord));
            builder.HasKey(record => record.Id);

            builder.HasIndex(record => record.BillNumber).IsUnique();
            builder.HasIndex(record => record.OrderId).HasFilter("OrderId IS NOT NULL").IsUnique();
            builder.HasIndex(record => record.Date);
        }
    }
}
