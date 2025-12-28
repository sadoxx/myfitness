using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFit.Infrastructure.Data;
using MyFit.Domain.Entities;

namespace MyFit.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProgressController(AppDbContext context)
    {
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpPost("weight")]
    public async Task<IActionResult> LogWeight([FromBody] LogWeightDto dto)
    {
        var userId = GetCurrentUserId();

        var weightLog = new WeightLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Weight = dto.Weight,
            LogDate = DateTime.SpecifyKind(dto.LogDate.Date, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.WeightLogs.Add(weightLog);
        await _context.SaveChangesAsync();

        return Ok(weightLog);
    }

    [HttpGet("weight")]
    public async Task<IActionResult> GetWeightLogs()
    {
        var userId = GetCurrentUserId();

        var logs = await _context.WeightLogs
            .Where(w => w.UserId == userId && !w.IsDeleted)
            .OrderByDescending(w => w.LogDate)
            .Take(30)
            .Select(w => new
            {
                w.Id,
                w.Weight,
                w.LogDate
            })
            .ToListAsync();

        return Ok(logs);
    }

    [HttpGet("weekly-stats")]
    public async Task<IActionResult> GetWeeklyStats()
    {
        var userId = GetCurrentUserId();
        var weekAgo = DateTime.UtcNow.AddDays(-7);

        var mealsLogged = await _context.MealLogs
            .Where(m => m.UserId == userId && m.LogDate >= weekAgo && !m.IsDeleted)
            .CountAsync();

        var workoutsCompleted = await _context.WorkoutLogs
            .Where(w => w.UserId == userId && w.WorkoutDate >= weekAgo && !w.IsDeleted)
            .CountAsync();

        var dailyCalories = await _context.MealLogs
            .Where(m => m.UserId == userId && m.LogDate >= weekAgo && !m.IsDeleted)
            .GroupBy(m => m.LogDate.Date)
            .Select(g => g.Sum(m => m.TotalCalories))
            .ToListAsync();

        var avgDailyCalories = dailyCalories.Any() ? dailyCalories.Average() : 0;

        var totalExerciseMinutes = await _context.WorkoutLogs
            .Where(w => w.UserId == userId && w.WorkoutDate >= weekAgo && !w.IsDeleted)
            .SumAsync(w => w.DurationMinutes);

        return Ok(new
        {
            MealsLogged = mealsLogged,
            WorkoutsCompleted = workoutsCompleted,
            AvgDailyCalories = avgDailyCalories,
            TotalExerciseMinutes = totalExerciseMinutes
        });
    }

    [HttpGet("nutrition")]
    public async Task<IActionResult> GetNutritionProgress()
    {
        var userId = GetCurrentUserId();
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        var userProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(up => up.UserId == userId && !up.IsDeleted);

        var calorieGoal = userProfile?.DailyCalorieGoal ?? 2000;

        var nutritionData = await _context.MealLogs
            .Where(m => m.UserId == userId && m.LogDate >= sevenDaysAgo && !m.IsDeleted)
            .GroupBy(m => m.LogDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                Calories = g.Sum(m => m.TotalCalories),
                Protein = g.Sum(m => m.TotalProtein),
                Carbs = g.Sum(m => m.TotalCarbs),
                Fats = g.Sum(m => m.TotalFats),
                MealCount = g.Count(),
                CalorieGoal = calorieGoal
            })
            .OrderByDescending(n => n.Date)
            .ToListAsync();

        return Ok(nutritionData);
    }

    [HttpGet("workouts")]
    public async Task<IActionResult> GetRecentWorkouts()
    {
        var userId = GetCurrentUserId();

        var workouts = await _context.WorkoutLogs
            .Where(w => w.UserId == userId && !w.IsDeleted)
            .Include(w => w.ExerciseLogs)
            .OrderByDescending(w => w.WorkoutDate)
            .Take(10)
            .Select(w => new
            {
                w.Id,
                w.WorkoutName,
                w.WorkoutDate,
                w.DurationMinutes,
                ExerciseCount = w.ExerciseLogs.Count(e => !e.IsDeleted)
            })
            .ToListAsync();

        return Ok(workouts);
    }
}

public class LogWeightDto
{
    public decimal Weight { get; set; }
    public DateTime LogDate { get; set; }
}
