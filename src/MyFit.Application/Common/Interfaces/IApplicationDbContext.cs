using MyFit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MyFit.Application.Common.Interfaces;

/// <summary>
/// Interface for the database context - used in Application layer
/// </summary>
public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<WorkoutPlan> WorkoutPlans { get; }
    DbSet<WorkoutDay> WorkoutDays { get; }
    DbSet<WorkoutExercise> WorkoutExercises { get; }
    DbSet<FoodItem> FoodItems { get; }
    DbSet<MealLog> MealLogs { get; }
    DbSet<WaterIntake> WaterIntakes { get; }
    DbSet<SleepLog> SleepLogs { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
