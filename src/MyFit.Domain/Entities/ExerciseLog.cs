using MyFit.Domain.Common;

namespace MyFit.Domain.Entities;

/// <summary>
/// Represents an exercise performed in a workout session
/// </summary>
public class ExerciseLog : BaseEntity
{
    public Guid WorkoutLogId { get; set; }
    public WorkoutLog WorkoutLog { get; set; } = null!;
    
    public Guid ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    
    public int OrderIndex { get; set; }
    
    // Navigation
    public ICollection<SetLog> Sets { get; set; } = new List<SetLog>();
}
