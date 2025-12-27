using MediatR;
using MyFit.Application.Common.Models;
using MyFit.Domain.Enums;

namespace MyFit.Application.Workouts.Queries;

/// <summary>
/// Query to get all exercises (from seeded database)
/// </summary>
public record GetAllExercisesQuery : IRequest<Result<List<ExerciseDto>>>
{
    public MuscleGroup? FilterByMuscleGroup { get; init; }
    public Difficulty? FilterByDifficulty { get; init; }
}

public class ExerciseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MuscleGroup MuscleGroup { get; set; }
    public Difficulty Difficulty { get; set; }
    public string? ImageUrl { get; set; }
    public string? Instructions { get; set; }
}
