using MyFit.Domain.Common;
using MyFit.Domain.Enums;

namespace MyFit.Domain.Entities;

/// <summary>
/// Seeded exercise database (loaded from exercises.json on startup)
/// </summary>
public class Exercise : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MuscleGroup MuscleGroup { get; set; }
    public Difficulty Difficulty { get; set; }
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    
    // Instructions
    public string? Instructions { get; set; }
    
    // Navigation
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; } = new List<WorkoutExercise>();
}
