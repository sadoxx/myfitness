using MyFit.Domain.Common;

namespace MyFit.Domain.Entities;

/// <summary>
/// Daily water intake tracking
/// </summary>
public class WaterIntake : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public decimal Amount { get; set; } // in milliliters
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}
