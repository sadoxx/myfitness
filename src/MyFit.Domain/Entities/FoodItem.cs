using MyFit.Domain.Common;

namespace MyFit.Domain.Entities;

/// <summary>
/// Local food database - cached from OpenFoodFacts API
/// Implements the Hybrid API strategy
/// </summary>
public class FoodItem : BaseEntity
{
    // External API Reference
    public string? ExternalId { get; set; } // OpenFoodFacts product code
    public string? ExternalSource { get; set; } // "OpenFoodFacts", "Manual", etc.
    
    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Barcode { get; set; }
    
    // Nutritional Information (per 100g or per serving)
    public decimal ServingSize { get; set; } // in grams
    public decimal Calories { get; set; }
    public decimal Protein { get; set; } // in grams
    public decimal Carbs { get; set; } // in grams
    public decimal Fats { get; set; } // in grams
    public decimal Fiber { get; set; } // in grams
    public decimal Sugar { get; set; } // in grams
    
    // Additional Information
    public string? ImageUrl { get; set; }
    public string? Category { get; set; }
    
    // Usage Tracking
    public int TimesLogged { get; set; } // How many times users logged this food
    public DateTime? LastUsedAt { get; set; }
    
    // Navigation
    public ICollection<MealLog> MealLogs { get; set; } = new List<MealLog>();
}
