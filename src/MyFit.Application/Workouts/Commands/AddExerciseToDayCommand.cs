using MediatR;
using MyFit.Application.Common.Models;
using FluentValidation;

namespace MyFit.Application.Workouts.Commands;

/// <summary>
/// Command to add an exercise to a workout day
/// </summary>
public record AddExerciseToDayCommand : IRequest<Result<Guid>>
{
    public Guid WorkoutDayId { get; init; }
    public Guid ExerciseId { get; init; }
    public int Sets { get; init; }
    public int Reps { get; init; }
    public decimal? Weight { get; init; }
    public int RestTime { get; init; } = 60;
    public string? Notes { get; init; }
}

public class AddExerciseToDayCommandValidator : AbstractValidator<AddExerciseToDayCommand>
{
    public AddExerciseToDayCommandValidator()
    {
        RuleFor(x => x.WorkoutDayId)
            .NotEmpty().WithMessage("Workout day ID is required");

        RuleFor(x => x.ExerciseId)
            .NotEmpty().WithMessage("Exercise ID is required");

        RuleFor(x => x.Sets)
            .GreaterThan(0).WithMessage("Sets must be greater than 0")
            .LessThan(100).WithMessage("Sets must be less than 100");

        RuleFor(x => x.Reps)
            .GreaterThan(0).WithMessage("Reps must be greater than 0")
            .LessThan(1000).WithMessage("Reps must be less than 1000");

        RuleFor(x => x.Weight)
            .GreaterThan(0).When(x => x.Weight.HasValue)
            .WithMessage("Weight must be greater than 0");

        RuleFor(x => x.RestTime)
            .GreaterThanOrEqualTo(0).WithMessage("Rest time must be non-negative");
    }
}
