using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyFit.Domain.Entities;

namespace MyFit.Infrastructure.Data.Configurations;

public class WorkoutDayConfiguration : IEntityTypeConfiguration<WorkoutDay>
{
    public void Configure(EntityTypeBuilder<WorkoutDay> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DayName)
            .HasMaxLength(50);

        // One-to-Many with WorkoutExercises
        builder.HasMany(d => d.Exercises)
            .WithOne(e => e.WorkoutDay)
            .HasForeignKey(e => e.WorkoutDayId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
