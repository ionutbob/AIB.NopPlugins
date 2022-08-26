using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.BlockMaliciousRequests.Domain;

namespace Nop.Plugin.Misc.BlockMaliciousRequests.Data
{
    public class RequestLogRecordMap : NopEntityTypeConfiguration<RequestLogRecord>
    {
        public override void Configure(EntityTypeBuilder<RequestLogRecord> builder)
        {
            builder.ToTable(nameof(RequestLogRecord));
            builder.HasKey(record => record.Id);

            builder.HasIndex(record => record.Ip);
            builder.HasIndex(record => record.Country);
            builder.HasIndex(record => record.RequestTime);
            builder.HasIndex(record => record.BlockedBy);
        }
    }
}
