using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyFit.Domain.Entities;

namespace MyFit.Infrastructure.Data.Configurations;

public class WaterIntakeConfiguration : IEntityTypeConfiguration<WaterIntake>
{
    public void Configure(EntityTypeBuilder<WaterIntake> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Amount)
            .HasPrecision(8, 2);

        // Index for date-based queries
        builder.HasIndex(w => new { w.UserId, w.Date });
    }
}
