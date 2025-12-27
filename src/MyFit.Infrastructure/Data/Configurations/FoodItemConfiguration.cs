using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyFit.Domain.Entities;

namespace MyFit.Infrastructure.Data.Configurations;

public class FoodItemConfiguration : IEntityTypeConfiguration<FoodItem>
{
    public void Configure(EntityTypeBuilder<FoodItem> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Brand)
            .HasMaxLength(100);

        builder.Property(f => f.ExternalId)
            .HasMaxLength(100);

        builder.Property(f => f.ExternalSource)
            .HasMaxLength(50);

        builder.Property(f => f.Category)
            .HasMaxLength(100);

        // Precision for nutritional values
        builder.Property(f => f.ServingSize).HasPrecision(8, 2);
        builder.Property(f => f.Calories).HasPrecision(8, 2);
        builder.Property(f => f.Protein).HasPrecision(8, 2);
        builder.Property(f => f.Carbs).HasPrecision(8, 2);
        builder.Property(f => f.Fats).HasPrecision(8, 2);
        builder.Property(f => f.Fiber).HasPrecision(8, 2);
        builder.Property(f => f.Sugar).HasPrecision(8, 2);

        // Unique index for external ID (Hybrid API strategy)
        builder.HasIndex(f => new { f.ExternalId, f.ExternalSource })
            .IsUnique()
            .HasFilter("\"ExternalId\" IS NOT NULL");

        // Index for searching
        builder.HasIndex(f => f.Name);
    }
}
