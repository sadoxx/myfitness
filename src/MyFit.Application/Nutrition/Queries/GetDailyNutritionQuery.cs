using MediatR;
using MyFit.Application.Common.Models;

namespace MyFit.Application.Nutrition.Queries;

/// <summary>
/// Query to get daily nutrition summary for dashboard
/// </summary>
public record GetDailyNutritionQuery : IRequest<Result<DailyNutritionDto>>
{
    public Guid UserId { get; init; }
    public DateTime Date { get; init; }
}

public class DailyNutritionDto
{
    public decimal TotalCalories { get; set; }
    public decimal TotalProtein { get; set; }
    public decimal TotalCarbs { get; set; }
    public decimal TotalFats { get; set; }
    
    // Goals from UserProfile
    public decimal CalorieGoal { get; set; }
    public decimal ProteinGoal { get; set; }
    public decimal CarbsGoal { get; set; }
    public decimal FatsGoal { get; set; }
    
    // Water intake
    public decimal TotalWater { get; set; } // in ml
    public decimal WaterGoal { get; set; } = 2000; // default 2 liters
    
    // Calculations for dashboard
    public decimal CaloriesRemaining => Math.Max(0, CalorieGoal - TotalCalories);
    public decimal ProteinPercentage => CalorieGoal > 0 ? (TotalProtein * 4 / CalorieGoal) * 100 : 0;
    public decimal CarbsPercentage => CalorieGoal > 0 ? (TotalCarbs * 4 / CalorieGoal) * 100 : 0;
    public decimal FatsPercentage => CalorieGoal > 0 ? (TotalFats * 9 / CalorieGoal) * 100 : 0;
}
