using MediatR;
using MyFit.Application.Common.Models;
using FluentValidation;

namespace MyFit.Application.Workouts.Commands;

/// <summary>
/// Command to create a new workout plan
/// </summary>
public record CreateWorkoutPlanCommand : IRequest<Result<Guid>>
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
}

public class CreateWorkoutPlanCommandValidator : AbstractValidator<CreateWorkoutPlanCommand>
{
    public CreateWorkoutPlanCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plan name is required")
            .MaximumLength(100).WithMessage("Plan name must not exceed 100 characters");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");
    }
}
