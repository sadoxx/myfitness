using MediatR;
using MyFit.Application.Common.Models;
using MyFit.Domain.Enums;
using FluentValidation;

namespace MyFit.Application.Auth.Commands;

/// <summary>
/// Command to complete the onboarding wizard and calculate BMR/TDEE
/// </summary>
public record CompleteOnboardingCommand : IRequest<Result<OnboardingResponse>>
{
    public Guid UserId { get; init; }
    public Gender Gender { get; init; }
    public int Age { get; init; }
    public decimal CurrentWeight { get; init; } // kg
    public decimal Height { get; init; } // cm
    public FitnessGoal PrimaryGoal { get; init; }
    public ActivityLevel ActivityLevel { get; init; }
}

public class OnboardingResponse
{
    public decimal BMR { get; set; }
    public decimal TDEE { get; set; }
    public decimal DailyCalorieGoal { get; set; }
    public decimal DailyProteinGoal { get; set; }
    public decimal DailyCarbsGoal { get; set; }
    public decimal DailyFatsGoal { get; set; }
}

public class CompleteOnboardingCommandValidator : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingCommandValidator()
    {
        RuleFor(x => x.Age)
            .GreaterThan(0).WithMessage("Age must be greater than 0")
            .LessThan(120).WithMessage("Age must be less than 120");

        RuleFor(x => x.CurrentWeight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0")
            .LessThan(500).WithMessage("Weight must be less than 500kg");

        RuleFor(x => x.Height)
            .GreaterThan(0).WithMessage("Height must be greater than 0")
            .LessThan(300).WithMessage("Height must be less than 300cm");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender value");

        RuleFor(x => x.PrimaryGoal)
            .IsInEnum().WithMessage("Invalid fitness goal");

        RuleFor(x => x.ActivityLevel)
            .IsInEnum().WithMessage("Invalid activity level");
    }
}
