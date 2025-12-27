using MyFit.Domain.Common;

namespace MyFit.Domain.Entities;

/// <summary>
/// Represents a specific day in a workout plan (e.g., Monday, Tuesday)
/// </summary>
public class WorkoutDay : BaseEntity
{
    public Guid WorkoutPlanId { get; set; }
    public WorkoutPlan WorkoutPlan { get; set; } = null!;
    
    public DayOfWeek DayOfWeek { get; set; }
    public string? DayName { get; set; } // e.g., "Chest Day", "Leg Day"
    public int OrderIndex { get; set; } // For sorting
    
    // Navigation
    public ICollection<WorkoutExercise> Exercises { get; set; } = new List<WorkoutExercise>();
}
