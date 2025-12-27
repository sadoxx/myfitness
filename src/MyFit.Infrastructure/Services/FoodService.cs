using Microsoft.EntityFrameworkCore;
using MyFit.Application.Common.Interfaces;
using MyFit.Domain.Entities;
using MyFit.Infrastructure.Data;
using System.Text.Json;

namespace MyFit.Infrastructure.Services;

/// <summary>
/// Food Service implementing the Hybrid API Strategy:
/// 1. Search OpenFoodFacts API
/// 2. Check if food exists locally by ExternalId
/// 3. If not, cache it locally
/// 4. Return local FoodItem ID
/// </summary>
public class FoodService : IFoodService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private const string OpenFoodFactsApiUrl = "https://world.openfoodfacts.org/cgi/search.pl";

    public FoodService(AppDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
    }

    /// <summary>
    /// Search for food items from OpenFoodFacts API
    /// </summary>
    public async Task<List<FoodSearchResult>> SearchFoodItemsAsync(string query, int maxResults = 20)
    {
        try
        {
            var url = $"{OpenFoodFactsApiUrl}?search_terms={Uri.EscapeDataString(query)}&search_simple=1&json=1&page_size={maxResults}";
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var openFoodResponse = JsonSerializer.Deserialize<OpenFoodFactsResponse>(jsonString, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (openFoodResponse?.Products == null)
                return new List<FoodSearchResult>();

            var results = openFoodResponse.Products
                .Where(p => !string.IsNullOrEmpty(p.Code) && !string.IsNullOrEmpty(p.ProductName))
                .Select(p => new FoodSearchResult
                {
                    ExternalId = p.Code,
                    Name = p.ProductName ?? "Unknown",
                    Brand = p.Brands,
                    Calories = ParseDecimal(p.EnergyKcal100g),
                    Protein = ParseDecimal(p.Proteins100g),
                    Carbs = ParseDecimal(p.Carbohydrates100g),
                    Fats = ParseDecimal(p.Fat100g),
                    ServingSize = 100, // OpenFoodFacts uses per 100g
                    ImageUrl = p.ImageUrl
                })
                .Take(maxResults)
                .ToList();

            return results;
        }
        catch (Exception ex)
        {
            // Log error and return empty list (graceful degradation)
            Console.WriteLine($"Error searching OpenFoodFacts: {ex.Message}");
            return new List<FoodSearchResult>();
        }
    }

    /// <summary>
    /// Get or create a local FoodItem from external source (Hybrid Strategy)
    /// This is the KEY method that implements the caching logic
    /// </summary>
    public async Task<Guid> GetOrCreateLocalFoodItemAsync(string externalId, string externalSource)
    {
        // Step 1: Check if food item already exists in local database
        var existingFoodItem = await _context.FoodItems
            .FirstOrDefaultAsync(f => f.ExternalId == externalId && f.ExternalSource == externalSource);

        if (existingFoodItem != null)
        {
            // Food item already cached - update usage stats
            existingFoodItem.TimesLogged++;
            existingFoodItem.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingFoodItem.Id;
        }

        // Step 2: Food item not in local DB - fetch from external API
        if (externalSource == "OpenFoodFacts")
        {
            var foodData = await FetchFromOpenFoodFactsAsync(externalId);
            
            if (foodData == null)
                throw new Exception($"Could not fetch food item from OpenFoodFacts: {externalId}");

            // Step 3: Create and save to local database
            var newFoodItem = new FoodItem
            {
                Id = Guid.NewGuid(),
                ExternalId = externalId,
                ExternalSource = externalSource,
                Name = foodData.Name,
                Brand = foodData.Brand,
                ServingSize = foodData.ServingSize,
                Calories = foodData.Calories,
                Protein = foodData.Protein,
                Carbs = foodData.Carbs,
                Fats = foodData.Fats,
                Fiber = 0, // OpenFoodFacts might have this
                Sugar = 0,
                ImageUrl = foodData.ImageUrl,
                TimesLogged = 1,
                LastUsedAt = DateTime.UtcNow
            };

            _context.FoodItems.Add(newFoodItem);
            await _context.SaveChangesAsync();

            return newFoodItem.Id;
        }

        throw new NotSupportedException($"External source '{externalSource}' is not supported");
    }

    /// <summary>
    /// Fetch detailed food data from OpenFoodFacts by product code
    /// </summary>
    private async Task<FoodSearchResult?> FetchFromOpenFoodFactsAsync(string productCode)
    {
        try
        {
            var url = $"https://world.openfoodfacts.org/api/v0/product/{productCode}.json";
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var productResponse = JsonSerializer.Deserialize<OpenFoodFactsProductResponse>(jsonString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (productResponse?.Product == null)
                return null;

            var p = productResponse.Product;
            return new FoodSearchResult
            {
                ExternalId = productCode,
                Name = p.ProductName ?? "Unknown",
                Brand = p.Brands,
                Calories = ParseDecimal(p.EnergyKcal100g),
                Protein = ParseDecimal(p.Proteins100g),
                Carbs = ParseDecimal(p.Carbohydrates100g),
                Fats = ParseDecimal(p.Fat100g),
                ServingSize = 100,
                ImageUrl = p.ImageUrl
            };
        }
        catch
        {
            return null;
        }
    }

    private static decimal ParseDecimal(object? value)
    {
        if (value == null) return 0;
        
        if (value is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Number)
                return jsonElement.GetDecimal();
            if (jsonElement.ValueKind == JsonValueKind.String)
                return decimal.TryParse(jsonElement.GetString(), out var result) ? result : 0;
        }

        return decimal.TryParse(value.ToString(), out var parsed) ? parsed : 0;
    }
}

#region OpenFoodFacts DTOs

public class OpenFoodFactsResponse
{
    public List<OpenFoodFactsProduct>? Products { get; set; }
}

public class OpenFoodFactsProductResponse
{
    public OpenFoodFactsProduct? Product { get; set; }
}

public class OpenFoodFactsProduct
{
    public string? Code { get; set; }
    public string? ProductName { get; set; }
    public string? Brands { get; set; }
    public object? EnergyKcal100g { get; set; }
    public object? Proteins100g { get; set; }
    public object? Carbohydrates100g { get; set; }
    public object? Fat100g { get; set; }
    public string? ImageUrl { get; set; }
}

#endregion
