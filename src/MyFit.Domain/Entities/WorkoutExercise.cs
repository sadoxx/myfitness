using MyFit.Domain.Common;

namespace MyFit.Domain.Entities;

/// <summary>
/// Join table between WorkoutDay and Exercise with sets/reps information
/// </summary>
public class WorkoutExercise : BaseEntity
{
    public Guid WorkoutDayId { get; set; }
    public WorkoutDay WorkoutDay { get; set; } = null!;
    
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    
    // Exercise Details
    public int Sets { get; set; }
    public int Reps { get; set; }
    public decimal? Weight { get; set; } // Optional weight in kg
    public int RestTime { get; set; } // Rest time in seconds
    public int OrderIndex { get; set; } // Order in the workout
    public string? Notes { get; set; }
}
