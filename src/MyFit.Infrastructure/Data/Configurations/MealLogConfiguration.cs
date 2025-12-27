using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyFit.Domain.Entities;

namespace MyFit.Infrastructure.Data.Configurations;

public class MealLogConfiguration : IEntityTypeConfiguration<MealLog>
{
    public void Configure(EntityTypeBuilder<MealLog> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.QuantityUnit)
            .HasMaxLength(20);

        builder.Property(m => m.Notes)
            .HasMaxLength(500);

        // Precision for calculated values
        builder.Property(m => m.Quantity).HasPrecision(8, 2);
        builder.Property(m => m.TotalCalories).HasPrecision(8, 2);
        builder.Property(m => m.TotalProtein).HasPrecision(8, 2);
        builder.Property(m => m.TotalCarbs).HasPrecision(8, 2);
        builder.Property(m => m.TotalFats).HasPrecision(8, 2);

        // Relationship with FoodItem
        builder.HasOne(m => m.FoodItem)
            .WithMany(f => f.MealLogs)
            .HasForeignKey(m => m.FoodItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index for date-based queries
        builder.HasIndex(m => new { m.UserId, m.LogDate });
    }
}
