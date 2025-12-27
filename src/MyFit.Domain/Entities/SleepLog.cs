using MyFit.Domain.Common;

namespace MyFit.Domain.Entities;

/// <summary>
/// Sleep tracking for wellness monitoring
/// </summary>
public class SleepLog : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public DateTime BedTime { get; set; }
    public DateTime WakeUpTime { get; set; }
    public decimal TotalHours { get; set; }
    
    // Sleep Quality (1-5 scale)
    public int? QualityRating { get; set; }
    public string? Notes { get; set; }
}
