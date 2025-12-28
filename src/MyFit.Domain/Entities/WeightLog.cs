using MyFit.Domain.Common;

namespace MyFit.Domain.Entities;

public class WeightLog : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public decimal Weight { get; set; } // in kilograms
    public DateTime LogDate { get; set; }
    public string? Notes { get; set; }
}
