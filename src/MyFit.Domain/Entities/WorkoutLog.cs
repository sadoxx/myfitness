using MyFit.Domain.Common;

namespace MyFit.Domain.Entities;

/// <summary>
/// Represents a completed workout session
/// </summary>
public class WorkoutLog : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public DateTime WorkoutDate { get; set; }
    public string? WorkoutName { get; set; }
    public int DurationMinutes { get; set; }
    public string? Notes { get; set; }
    
    // Navigation
    public ICollection<ExerciseLog> ExerciseLogs { get; set; } = new List<ExerciseLog>();
}
