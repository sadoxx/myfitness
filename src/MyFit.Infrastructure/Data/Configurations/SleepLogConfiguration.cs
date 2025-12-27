using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyFit.Domain.Entities;

namespace MyFit.Infrastructure.Data.Configurations;

public class SleepLogConfiguration : IEntityTypeConfiguration<SleepLog>
{
    public void Configure(EntityTypeBuilder<SleepLog> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.TotalHours)
            .HasPrecision(4, 2);

        builder.Property(s => s.Notes)
            .HasMaxLength(500);

        // Index for date-based queries
        builder.HasIndex(s => new { s.UserId, s.Date });
    }
}
