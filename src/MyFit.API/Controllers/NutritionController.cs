using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFit.Application.Common.Interfaces;
using MyFit.Application.Nutrition.Commands;
using MyFit.Application.Nutrition.Queries;
using System.Security.Claims;

namespace MyFit.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NutritionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFoodService _foodService;
    private readonly IApplicationDbContext _context;

    public NutritionController(
        IMediator mediator,
        IFoodService foodService,
        IApplicationDbContext context)
    {
        _mediator = mediator;
        _foodService = foodService;
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [AllowAnonymous]
    [HttpGet("search")]
    public async Task<IActionResult> SearchFoodItems([FromQuery] string query, [FromQuery] int maxResults = 20)
    {
        var results = await _foodService.SearchFoodItemsAsync(query, maxResults);
        return Ok(results);
    }

    [HttpPost("meal-log")]
    public async Task<IActionResult> AddMealLog([FromBody] AddMealLogRequest request)
    {
        // First, ensure the food item is cached locally (Hybrid API strategy)
        var localFoodItemId = await _foodService.GetOrCreateLocalFoodItemAsync(
            request.ExternalId, 
            request.ExternalSource);

        // Get food item to calculate totals
        var foodItem = await _context.FoodItems.FindAsync(localFoodItemId);
        if (foodItem == null)
        {
            return BadRequest(new { Message = "Food item not found" });
        }

        // Calculate nutritional values based on quantity
        var multiplier = request.Quantity;
        var command = new AddMealLogCommand
        {
            UserId = GetCurrentUserId(),
            FoodItemId = localFoodItemId,
            MealType = (MyFit.Domain.Enums.MealType)request.MealType,
            Quantity = request.Quantity,
            QuantityUnit = request.QuantityUnit,
            LogDate = request.LogDate ?? DateTime.UtcNow,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(new { MealLogId = result.Data, Message = "Meal logged successfully" });
    }

    [HttpGet("daily-summary")]
    public async Task<IActionResult> GetDailyNutrition([FromQuery] DateTime? date)
    {
        var userId = GetCurrentUserId();
        var targetDate = (date ?? DateTime.UtcNow).Date;

        // Get meal logs for the day
        var mealLogs = await _context.MealLogs
            .Where(m => m.UserId == userId && m.LogDate.Date == targetDate)
            .ToListAsync();

        // Get user profile for goals
        var userProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (userProfile == null)
        {
            return NotFound(new { Message = "User profile not found" });
        }

        // Calculate totals
        var totalCalories = mealLogs.Sum(m => m.TotalCalories);
        var totalProtein = mealLogs.Sum(m => m.TotalProtein);
        var totalCarbs = mealLogs.Sum(m => m.TotalCarbs);
        var totalFats = mealLogs.Sum(m => m.TotalFats);

        // Get water intake
        var waterIntakes = await _context.WaterIntakes
            .Where(w => w.UserId == userId && w.Date.Date == targetDate)
            .ToListAsync();

        var totalWater = waterIntakes.Sum(w => w.Amount);

        var response = new DailyNutritionDto
        {
            TotalCalories = totalCalories,
            TotalProtein = totalProtein,
            TotalCarbs = totalCarbs,
            TotalFats = totalFats,
            CalorieGoal = userProfile.DailyCalorieGoal,
            ProteinGoal = userProfile.DailyProteinGoal,
            CarbsGoal = userProfile.DailyCarbsGoal,
            FatsGoal = userProfile.DailyFatsGoal,
            TotalWater = totalWater,
            WaterGoal = 2000
        };

        return Ok(response);
    }

    [HttpPost("water")]
    public async Task<IActionResult> AddWaterIntake([FromBody] AddWaterIntakeRequest request)
    {
        var command = new AddWaterIntakeCommand
        {
            UserId = GetCurrentUserId(),
            Amount = request.Amount,
            Date = request.Date ?? DateTime.UtcNow
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(new { WaterIntakeId = result.Data, Message = "Water intake logged" });
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetNutritionSummary([FromQuery] DateTime? date)
    {
        var userId = GetCurrentUserId();
        var targetDate = (date ?? DateTime.UtcNow).Date;

        // Get meal logs for the day
        var mealLogs = await _context.MealLogs
            .Where(m => m.UserId == userId && m.LogDate.Date == targetDate)
            .ToListAsync();

        var response = new
        {
            TotalCalories = mealLogs.Sum(m => m.TotalCalories),
            TotalProtein = mealLogs.Sum(m => m.TotalProtein),
            TotalCarbs = mealLogs.Sum(m => m.TotalCarbs),
            TotalFats = mealLogs.Sum(m => m.TotalFats)
        };

        return Ok(response);
    }

    [HttpGet("meal-logs")]
    public async Task<IActionResult> GetMealLogs([FromQuery] DateTime? date)
    {
        var userId = GetCurrentUserId();
        var targetDate = (date ?? DateTime.UtcNow).Date;

        var mealLogs = await _context.MealLogs
            .Include(m => m.FoodItem)
            .Where(m => m.UserId == userId && m.LogDate.Date == targetDate)
            .OrderByDescending(m => m.LogDate)
            .Select(m => new
            {
                m.Id,
                FoodName = m.FoodItem.Name,
                m.MealType,
                m.Quantity,
                m.QuantityUnit,
                m.TotalCalories,
                m.TotalProtein,
                m.TotalCarbs,
                m.TotalFats,
                m.LogDate
            })
            .ToListAsync();

        return Ok(mealLogs);
    }

    [HttpDelete("meal-logs/{id}")]
    public async Task<IActionResult> DeleteMealLog(Guid id)
    {
        var userId = GetCurrentUserId();
        var mealLog = await _context.MealLogs
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (mealLog == null)
        {
            return NotFound(new { Message = "Meal log not found" });
        }

        _context.MealLogs.Remove(mealLog);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Meal log deleted" });
    }

    [HttpPut("meal-logs/{id}")]
    public async Task<IActionResult> UpdateMealLog(Guid id, [FromBody] UpdateMealLogRequest request)
    {
        var userId = GetCurrentUserId();
        var mealLog = await _context.MealLogs
            .Include(m => m.FoodItem)
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (mealLog == null)
        {
            return NotFound(new { Message = "Meal log not found" });
        }

        // Recalculate nutritional values based on new quantity
        // Food item values are per 100g, so divide quantity by 100
        var multiplier = request.Quantity / 100m;
        mealLog.Quantity = request.Quantity;
        mealLog.MealType = (MyFit.Domain.Enums.MealType)request.MealType;
        mealLog.TotalCalories = mealLog.FoodItem.Calories * multiplier;
        mealLog.TotalProtein = mealLog.FoodItem.Protein * multiplier;
        mealLog.TotalCarbs = mealLog.FoodItem.Carbs * multiplier;
        mealLog.TotalFats = mealLog.FoodItem.Fats * multiplier;

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Meal log updated" });
    }
}

public class AddMealLogRequest
{
    public string ExternalId { get; set; } = string.Empty;
    public string ExternalSource { get; set; } = "OpenFoodFacts";
    public int MealType { get; set; }
    public decimal Quantity { get; set; }
    public string QuantityUnit { get; set; } = "serving";
    public DateTime? LogDate { get; set; }
    public string? Notes { get; set; }
}

public class AddWaterIntakeRequest
{
    public decimal Amount { get; set; }
    public DateTime? Date { get; set; }
}

public class UpdateMealLogRequest
{
    public decimal Quantity { get; set; }
    public int MealType { get; set; }
}
