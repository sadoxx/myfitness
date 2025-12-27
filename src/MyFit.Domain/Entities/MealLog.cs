using MyFit.Domain.Common;
using MyFit.Domain.Enums;

namespace MyFit.Domain.Entities;

/// <summary>
/// Log entry for a meal - links User, Date, MealType, and FoodItem
/// </summary>
public class MealLog : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public Guid FoodItemId { get; set; }
    public FoodItem FoodItem { get; set; } = null!;
    
    public DateTime LogDate { get; set; } = DateTime.UtcNow;
    public MealType MealType { get; set; }
    
    // Portion Information
    public decimal Quantity { get; set; } // e.g., 1.5 servings or 150 grams
    public string QuantityUnit { get; set; } = "serving"; // "serving", "grams", "oz"
    
    // Calculated Nutritional Values (based on quantity)
    public decimal TotalCalories { get; set; }
    public decimal TotalProtein { get; set; }
    public decimal TotalCarbs { get; set; }
    public decimal TotalFats { get; set; }
    
    public string? Notes { get; set; }
}
