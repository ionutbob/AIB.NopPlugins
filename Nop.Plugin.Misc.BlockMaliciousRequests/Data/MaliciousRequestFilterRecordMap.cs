using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.BlockMaliciousRequests.Domain;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Data
{
    public partial class MaliciousRequestFilterRecordMap : NopEntityTypeConfiguration<MaliciousRequestFilterRecord>
    {
        public override void Configure(EntityTypeBuilder<MaliciousRequestFilterRecord> builder)
        {
            builder.ToTable(nameof(MaliciousRequestFilterRecord));
            builder.HasKey(record => record.Id);

            builder.HasIndex(record => record.IsUrlRequestFilter);
            builder.HasIndex(record => record.IsCountryFilter);
            builder.HasIndex(record => record.IsIpFilter);
            builder.HasIndex(record => record.Value);
        }
    }
}
