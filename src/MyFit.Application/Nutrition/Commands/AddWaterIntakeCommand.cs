using MediatR;
using MyFit.Application.Common.Models;
using FluentValidation;

namespace MyFit.Application.Nutrition.Commands;

/// <summary>
/// Command to add water intake
/// </summary>
public record AddWaterIntakeCommand : IRequest<Result<Guid>>
{
    public Guid UserId { get; init; }
    public decimal Amount { get; init; } // in milliliters
    public DateTime? Date { get; init; }
}

public class AddWaterIntakeCommandValidator : AbstractValidator<AddWaterIntakeCommand>
{
    public AddWaterIntakeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0")
            .LessThan(10000).WithMessage("Amount must be less than 10 liters");
    }
}
