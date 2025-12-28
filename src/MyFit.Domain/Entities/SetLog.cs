using MyFit.Domain.Common;

namespace MyFit.Domain.Entities;

/// <summary>
/// Represents a single set of an exercise
/// </summary>
public class SetLog : BaseEntity
{
    public Guid ExerciseLogId { get; set; }
    public ExerciseLog ExerciseLog { get; set; } = null!;
    
    public int SetNumber { get; set; }
    public int Reps { get; set; }
    public decimal? Weight { get; set; } // in kg
    public bool Completed { get; set; }
}
