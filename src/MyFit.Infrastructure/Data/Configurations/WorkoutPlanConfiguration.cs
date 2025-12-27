using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyFit.Domain.Entities;

namespace MyFit.Infrastructure.Data.Configurations;

public class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
{
    public void Configure(EntityTypeBuilder<WorkoutPlan> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.Description)
            .HasMaxLength(500);

        // One-to-Many with WorkoutDays
        builder.HasMany(w => w.WorkoutDays)
            .WithOne(d => d.WorkoutPlan)
            .HasForeignKey(d => d.WorkoutPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for active plans
        builder.HasIndex(w => new { w.UserId, w.IsActive });
    }
}
