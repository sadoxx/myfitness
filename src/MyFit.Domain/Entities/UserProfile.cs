using MyFit.Domain.Common;
using MyFit.Domain.Enums;

namespace MyFit.Domain.Entities;

/// <summary>
/// User's fitness profile with calculated BMR and TDEE
/// </summary>
public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    // Personal Information
    public Gender Gender { get; set; }
    public int Age { get; set; }
    public decimal CurrentWeight { get; set; } // in kg
    public decimal Height { get; set; } // in cm
    
    // Fitness Goals
    public FitnessGoal PrimaryGoal { get; set; }
    public ActivityLevel ActivityLevel { get; set; }
    
    // Calculated Values (Auto-calculated after onboarding)
    public decimal BMR { get; set; } // Basal Metabolic Rate
    public decimal TDEE { get; set; } // Total Daily Energy Expenditure
    public decimal DailyCalorieGoal { get; set; }
    public decimal DailyProteinGoal { get; set; } // in grams
    public decimal DailyCarbsGoal { get; set; } // in grams
    public decimal DailyFatsGoal { get; set; } // in grams
    
    // Onboarding Status
    public bool IsOnboardingComplete { get; set; }
    public DateTime? OnboardingCompletedAt { get; set; }
}
