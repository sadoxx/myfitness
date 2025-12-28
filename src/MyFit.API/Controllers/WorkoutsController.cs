using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFit.Application.Common.Interfaces;
using MyFit.Application.Workouts.Commands;
using MyFit.Application.Workouts.Queries;
using MyFit.Domain.Entities;
using System.Security.Claims;

namespace MyFit.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WorkoutsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public WorkoutsController(IMediator mediator, IApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [AllowAnonymous]
    [HttpGet("exercises")]
    public async Task<IActionResult> GetAllExercises([FromQuery] int? muscleGroup, [FromQuery] int? difficulty)
    {
        var exercises = await _context.Exercises
            .Where(e => !muscleGroup.HasValue || (int)e.MuscleGroup == muscleGroup)
            .Where(e => !difficulty.HasValue || (int)e.Difficulty == difficulty)
            .Select(e => new ExerciseDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                MuscleGroup = e.MuscleGroup,
                Difficulty = e.Difficulty,
                ImageUrl = e.ImageUrl,
                Instructions = e.Instructions
            })
            .ToListAsync();

        return Ok(exercises);
    }

    [HttpPost("workout-plan")]
    public async Task<IActionResult> CreateWorkoutPlan([FromBody] CreateWorkoutPlanCommand command)
    {
        command = command with { UserId = GetCurrentUserId() };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(new { WorkoutPlanId = result.Data, Message = "Workout plan created" });
    }

    [HttpGet("workout-plan")]
    public async Task<IActionResult> GetActiveWorkoutPlan()
    {
        var userId = GetCurrentUserId();

        var workoutPlan = await _context.WorkoutPlans
            .Include(w => w.WorkoutDays)
                .ThenInclude(d => d.Exercises)
                    .ThenInclude(e => e.Exercise)
            .Where(w => w.UserId == userId && w.IsActive)
            .OrderByDescending(w => w.CreatedAt)
            .FirstOrDefaultAsync();

        if (workoutPlan == null)
        {
            return NotFound(new { Message = "No active workout plan found" });
        }

        return Ok(workoutPlan);
    }

    [HttpPost("workout-day")]
    public async Task<IActionResult> CreateWorkoutDay([FromBody] CreateWorkoutDayRequest request)
    {
        var workoutDay = new WorkoutDay
        {
            Id = Guid.NewGuid(),
            WorkoutPlanId = request.WorkoutPlanId,
            DayOfWeek = request.DayOfWeek,
            DayName = request.DayName,
            OrderIndex = request.OrderIndex,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkoutDays.Add(workoutDay);
        await _context.SaveChangesAsync();

        return Ok(new { WorkoutDayId = workoutDay.Id, Message = "Workout day created" });
    }

    [HttpPost("workout-exercise")]
    public async Task<IActionResult> AddExerciseToDay([FromBody] AddExerciseToDayCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(new { WorkoutExerciseId = result.Data, Message = "Exercise added to workout day" });
    }

    [HttpPost("log")]
    public async Task<IActionResult> LogWorkout([FromBody] LogWorkoutRequest request)
    {
        var userId = GetCurrentUserId();

        var workoutLog = new WorkoutLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WorkoutDate = request.WorkoutDate,
            WorkoutName = request.WorkoutName,
            DurationMinutes = request.DurationMinutes,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkoutLogs.Add(workoutLog);

        foreach (var exerciseDto in request.ExerciseLogs)
        {
            var exerciseLog = new ExerciseLog
            {
                Id = Guid.NewGuid(),
                WorkoutLogId = workoutLog.Id,
                ExerciseId = exerciseDto.ExerciseId,
                OrderIndex = exerciseDto.OrderIndex,
                CreatedAt = DateTime.UtcNow
            };

            _context.ExerciseLogs.Add(exerciseLog);

            foreach (var setDto in exerciseDto.Sets)
            {
                var setLog = new SetLog
                {
                    Id = Guid.NewGuid(),
                    ExerciseLogId = exerciseLog.Id,
                    SetNumber = setDto.SetNumber,
                    Reps = setDto.Reps,
                    Weight = setDto.Weight,
                    Completed = setDto.Completed,
                    CreatedAt = DateTime.UtcNow
                };

                _context.SetLogs.Add(setLog);
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { WorkoutLogId = workoutLog.Id, Message = "Workout logged successfully" });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetWorkoutHistory()
    {
        var userId = GetCurrentUserId();

        var workouts = await _context.WorkoutLogs
            .Include(w => w.ExerciseLogs)
                .ThenInclude(e => e.Exercise)
            .Include(w => w.ExerciseLogs)
                .ThenInclude(e => e.Sets)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.WorkoutDate)
            .Take(20)
            .Select(w => new WorkoutHistoryDto
            {
                Id = w.Id,
                WorkoutName = w.WorkoutName ?? "Workout",
                WorkoutDate = w.WorkoutDate,
                DurationMinutes = w.DurationMinutes,
                ExerciseLogs = w.ExerciseLogs.Select(e => new ExerciseLogDto
                {
                    ExerciseId = e.ExerciseId,
                    ExerciseName = e.Exercise.Name,
                    OrderIndex = e.OrderIndex,
                    Sets = e.Sets.Select(s => new SetLogDto
                    {
                        SetNumber = s.SetNumber,
                        Reps = s.Reps,
                        Weight = s.Weight,
                        Completed = s.Completed
                    }).ToList()
                }).ToList()
            })
            .ToListAsync();

        return Ok(workouts);
    }
}

public class CreateWorkoutDayRequest
{
    public Guid WorkoutPlanId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public string? DayName { get; set; }
    public int OrderIndex { get; set; }
}

public class LogWorkoutRequest
{
    public DateTime WorkoutDate { get; set; }
    public string WorkoutName { get; set; } = "";
    public int DurationMinutes { get; set; }
    public List<ExerciseLogDto> ExerciseLogs { get; set; } = new();
}

public class ExerciseLogDto
{
    public Guid ExerciseId { get; set; }
    public string ExerciseName { get; set; } = "";
    public int OrderIndex { get; set; }
    public List<SetLogDto> Sets { get; set; } = new();
}

public class SetLogDto
{
    public int SetNumber { get; set; }
    public int Reps { get; set; }
    public decimal? Weight { get; set; }
    public bool Completed { get; set; }
}

public class WorkoutHistoryDto
{
    public Guid Id { get; set; }
    public string WorkoutName { get; set; } = "";
    public DateTime WorkoutDate { get; set; }
    public int DurationMinutes { get; set; }
    public List<ExerciseLogDto> ExerciseLogs { get; set; } = new();
}
