using MediatR;
using Microsoft.EntityFrameworkCore;
using MyFit.Application.Common.Interfaces;
using MyFit.Application.Common.Models;
using MyFit.Application.Nutrition.Commands;
using MyFit.Domain.Entities;

namespace MyFit.Application.Nutrition.Handlers;

public class AddMealLogCommandHandler : IRequestHandler<AddMealLogCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public AddMealLogCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(AddMealLogCommand request, CancellationToken cancellationToken)
    {
        var foodItem = await _context.FoodItems
            .FindAsync(new object[] { request.FoodItemId }, cancellationToken);

        if (foodItem == null)
        {
            return Result<Guid>.Failure("Food item not found");
        }

        // Calculate nutritional values based on quantity
        var multiplier = request.Quantity;
        var mealLog = new MealLog
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            FoodItemId = request.FoodItemId,
            LogDate = request.LogDate ?? DateTime.UtcNow,
            MealType = request.MealType,
            Quantity = request.Quantity,
            QuantityUnit = request.QuantityUnit,
            TotalCalories = foodItem.Calories * multiplier,
            TotalProtein = foodItem.Protein * multiplier,
            TotalCarbs = foodItem.Carbs * multiplier,
            TotalFats = foodItem.Fats * multiplier,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.MealLogs.Add(mealLog);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(mealLog.Id);
    }
}
