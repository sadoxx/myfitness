using Microsoft.AspNetCore.Identity;

namespace MyFit.Domain.Entities;

/// <summary>
/// Extended Identity User with navigation to UserProfile
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation Properties
    public UserProfile? UserProfile { get; set; }
    public ICollection<WorkoutPlan> WorkoutPlans { get; set; } = new List<WorkoutPlan>();
    public ICollection<MealLog> MealLogs { get; set; } = new List<MealLog>();
    public ICollection<WaterIntake> WaterIntakes { get; set; } = new List<WaterIntake>();
    public ICollection<SleepLog> SleepLogs { get; set; } = new List<SleepLog>();
}
