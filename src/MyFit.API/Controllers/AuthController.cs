using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyFit.Application.Auth.Commands;
using MyFit.Application.Common.Interfaces;
using MyFit.Domain.Entities;
using MyFit.Domain.Enums;

namespace MyFit.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IMediator mediator,
        IApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _mediator = mediator;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var user = new ApplicationUser
        {
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, command.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        // Create empty UserProfile
        var userProfile = new UserProfile
        {
            Id = user.Id,  // Use the same ID as the ApplicationUser
            UserId = user.Id,
            IsOnboardingComplete = false
        };

        _context.UserProfiles.Add(userProfile);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user.Id, user.Email!, user.FirstName);

        return Ok(new RegisterResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            Token = token
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);

        if (user == null)
        {
            return Unauthorized(new { Message = "Invalid email or password" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, command.Password, false);

        if (!result.Succeeded)
        {
            return Unauthorized(new { Message = "Invalid email or password" });
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var token = _tokenService.GenerateToken(user.Id, user.Email!, user.FirstName);

        // Check onboarding status
        var profile = await _context.UserProfiles.FindAsync(user.Id);
        var isOnboardingComplete = profile?.IsOnboardingComplete ?? false;

        return Ok(new LoginResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            Token = token,
            IsOnboardingComplete = isOnboardingComplete
        });
    }

    [HttpPost("onboarding")]
    [Authorize]
    public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingCommand command)
    {
        var userId = GetCurrentUserId();
        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);

        if (userProfile == null)
        {
            return NotFound(new { Message = "User profile not found" });
        }

        // Calculate BMR using Mifflin-St Jeor Equation
        decimal bmr;
        if (command.Gender == Gender.Male)
        {
            bmr = (10 * command.CurrentWeight) + (6.25m * command.Height) - (5 * command.Age) + 5;
        }
        else
        {
            bmr = (10 * command.CurrentWeight) + (6.25m * command.Height) - (5 * command.Age) - 161;
        }

        // Calculate TDEE based on activity level
        decimal activityMultiplier = command.ActivityLevel switch
        {
            ActivityLevel.Sedentary => 1.2m,
            ActivityLevel.LightlyActive => 1.375m,
            ActivityLevel.ModeratelyActive => 1.55m,
            ActivityLevel.VeryActive => 1.725m,
            ActivityLevel.ExtraActive => 1.9m,
            _ => 1.2m
        };

        decimal tdee = bmr * activityMultiplier;

        // Adjust calories based on goal
        decimal dailyCalorieGoal = command.PrimaryGoal switch
        {
            FitnessGoal.WeightLoss => tdee - 500,
            FitnessGoal.MuscleGain => tdee + 300,
            FitnessGoal.Maintenance => tdee,
            _ => tdee
        };

        // Calculate macro goals (standard 40/30/30 split for maintenance, adjusted for goals)
        decimal proteinGoal, carbsGoal, fatsGoal;

        if (command.PrimaryGoal == FitnessGoal.MuscleGain)
        {
            // Higher protein for muscle gain (40% protein, 35% carbs, 25% fats)
            proteinGoal = (dailyCalorieGoal * 0.40m) / 4;
            carbsGoal = (dailyCalorieGoal * 0.35m) / 4;
            fatsGoal = (dailyCalorieGoal * 0.25m) / 9;
        }
        else if (command.PrimaryGoal == FitnessGoal.WeightLoss)
        {
            // Higher protein for satiety (35% protein, 35% carbs, 30% fats)
            proteinGoal = (dailyCalorieGoal * 0.35m) / 4;
            carbsGoal = (dailyCalorieGoal * 0.35m) / 4;
            fatsGoal = (dailyCalorieGoal * 0.30m) / 9;
        }
        else
        {
            // Balanced (30% protein, 40% carbs, 30% fats)
            proteinGoal = (dailyCalorieGoal * 0.30m) / 4;
            carbsGoal = (dailyCalorieGoal * 0.40m) / 4;
            fatsGoal = (dailyCalorieGoal * 0.30m) / 9;
        }

        // Update user profile
        userProfile.Gender = command.Gender;
        userProfile.Age = command.Age;
        userProfile.CurrentWeight = command.CurrentWeight;
        userProfile.Height = command.Height;
        userProfile.PrimaryGoal = command.PrimaryGoal;
        userProfile.ActivityLevel = command.ActivityLevel;
        userProfile.BMR = Math.Round(bmr, 2);
        userProfile.TDEE = Math.Round(tdee, 2);
        userProfile.DailyCalorieGoal = Math.Round(dailyCalorieGoal, 2);
        userProfile.DailyProteinGoal = Math.Round(proteinGoal, 2);
        userProfile.DailyCarbsGoal = Math.Round(carbsGoal, 2);
        userProfile.DailyFatsGoal = Math.Round(fatsGoal, 2);
        userProfile.IsOnboardingComplete = true;
        userProfile.OnboardingCompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new OnboardingResponse
        {
            BMR = userProfile.BMR,
            TDEE = userProfile.TDEE,
            DailyCalorieGoal = userProfile.DailyCalorieGoal,
            DailyProteinGoal = userProfile.DailyProteinGoal,
            DailyCarbsGoal = userProfile.DailyCarbsGoal,
            DailyFatsGoal = userProfile.DailyFatsGoal
        });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);

        if (userProfile == null)
        {
            return NotFound(new { Message = "User profile not found" });
        }

        return Ok(new
        {
            userProfile.UserId,
            Gender = (int)userProfile.Gender,
            userProfile.Age,
            userProfile.CurrentWeight,
            userProfile.Height,
            PrimaryGoal = (int)userProfile.PrimaryGoal,
            ActivityLevel = (int)userProfile.ActivityLevel,
            userProfile.BMR,
            userProfile.TDEE,
            userProfile.DailyCalorieGoal,
            userProfile.DailyProteinGoal,
            userProfile.DailyCarbsGoal,
            userProfile.DailyFatsGoal,
            userProfile.IsOnboardingComplete
        });
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }}