using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyFit.Domain.Entities;

namespace MyFit.Infrastructure.Data.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.CurrentWeight)
            .HasPrecision(5, 2);

        builder.Property(p => p.Height)
            .HasPrecision(5, 2);

        builder.Property(p => p.BMR)
            .HasPrecision(7, 2);

        builder.Property(p => p.TDEE)
            .HasPrecision(7, 2);

        builder.Property(p => p.DailyCalorieGoal)
            .HasPrecision(7, 2);

        builder.Property(p => p.DailyProteinGoal)
            .HasPrecision(6, 2);

        builder.Property(p => p.DailyCarbsGoal)
            .HasPrecision(6, 2);

        builder.Property(p => p.DailyFatsGoal)
            .HasPrecision(6, 2);

        // Index for faster lookups
        builder.HasIndex(p => p.UserId)
            .IsUnique();
    }
}
