using MyFit.Domain.Common;

namespace MyFit.Domain.Entities;

/// <summary>
/// A user's workout plan (can have multiple weeks/days)
/// </summary>
public class WorkoutPlan : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation
    public ICollection<WorkoutDay> WorkoutDays { get; set; } = new List<WorkoutDay>();
}
