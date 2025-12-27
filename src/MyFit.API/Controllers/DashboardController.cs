using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFit.Application.Common.Interfaces;
using System.Security.Claims;

namespace MyFit.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IAIService _aiService;

    public DashboardController(IApplicationDbContext context, IAIService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetDashboardSummary([FromQuery] DateTime? date)
    {
        var userId = GetCurrentUserId();
        var targetDate = (date ?? DateTime.UtcNow).Date;

        // Get user profile
        var userProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (userProfile == null)
        {
            return NotFound(new { Message = "User profile not found" });
        }

        // Get nutrition data
        var mealLogs = await _context.MealLogs
            .Where(m => m.UserId == userId && m.LogDate.Date == targetDate)
            .ToListAsync();

        var waterIntakes = await _context.WaterIntakes
            .Where(w => w.UserId == userId && w.Date.Date == targetDate)
            .ToListAsync();

        var sleepLog = await _context.SleepLogs
            .Where(s => s.UserId == userId && s.Date.Date == targetDate)
            .FirstOrDefaultAsync();

        var summary = new
        {
            Profile = new
            {
                userProfile.DailyCalorieGoal,
                userProfile.DailyProteinGoal,
                userProfile.DailyCarbsGoal,
                userProfile.DailyFatsGoal
            },
            Nutrition = new
            {
                TotalCalories = mealLogs.Sum(m => m.TotalCalories),
                TotalProtein = mealLogs.Sum(m => m.TotalProtein),
                TotalCarbs = mealLogs.Sum(m => m.TotalCarbs),
                TotalFats = mealLogs.Sum(m => m.TotalFats),
                CaloriesRemaining = Math.Max(0, userProfile.DailyCalorieGoal - mealLogs.Sum(m => m.TotalCalories))
            },
            Water = new
            {
                TotalIntake = waterIntakes.Sum(w => w.Amount),
                Goal = 2000m,
                Percentage = Math.Min(100, (waterIntakes.Sum(w => w.Amount) / 2000m) * 100)
            },
            Sleep = sleepLog != null ? new
            {
                sleepLog.TotalHours,
                sleepLog.QualityRating,
                Goal = 8m
            } : null
        };

        return Ok(summary);
    }

    [HttpPost("ai-chat")]
    public async Task<IActionResult> ChatWithAI([FromBody] AIChatRequest request)
    {
        var response = await _aiService.GetFitnessAdviceAsync(request.Query, request.Context);
        return Ok(new { Response = response });
    }
}

public class AIChatRequest
{
    public string Query { get; set; } = string.Empty;
    public string? Context { get; set; }
}
