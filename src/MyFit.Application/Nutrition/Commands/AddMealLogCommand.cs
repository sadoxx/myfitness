using MediatR;
using MyFit.Application.Common.Models;
using MyFit.Domain.Enums;
using FluentValidation;

namespace MyFit.Application.Nutrition.Commands;

/// <summary>
/// Command to add a meal log entry
/// </summary>
public record AddMealLogCommand : IRequest<Result<Guid>>
{
    public Guid UserId { get; init; }
    public Guid FoodItemId { get; init; }
    public MealType MealType { get; init; }
    public decimal Quantity { get; init; }
    public string QuantityUnit { get; init; } = "serving";
    public DateTime? LogDate { get; init; }
    public string? Notes { get; init; }
}

public class AddMealLogCommandValidator : AbstractValidator<AddMealLogCommand>
{
    public AddMealLogCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.FoodItemId)
            .NotEmpty().WithMessage("Food Item ID is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0")
            .LessThan(100).WithMessage("Quantity must be less than 100");

        RuleFor(x => x.MealType)
            .IsInEnum().WithMessage("Invalid meal type");
    }
}
