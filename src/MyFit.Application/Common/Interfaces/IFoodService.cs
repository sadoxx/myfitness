namespace MyFit.Application.Common.Interfaces;

/// <summary>
/// Interface for Food Service implementing Hybrid API strategy
/// </summary>
public interface IFoodService
{
    /// <summary>
    /// Search for food items from OpenFoodFacts API
    /// </summary>
    Task<List<FoodSearchResult>> SearchFoodItemsAsync(string query, int maxResults = 20);
    
    /// <summary>
    /// Get or create a local FoodItem from external source
    /// Implements the caching strategy
    /// </summary>
    Task<Guid> GetOrCreateLocalFoodItemAsync(string externalId, string externalSource);
}

public class FoodSearchResult
{
    public string ExternalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbs { get; set; }
    public decimal Fats { get; set; }
    public decimal ServingSize { get; set; }
    public string? ImageUrl { get; set; }
}
