using Microsoft.EntityFrameworkCore;
using MyFit.Application.Common.Interfaces;
using MyFit.Domain.Entities;
using MyFit.Infrastructure.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyFit.Infrastructure.Services;

public class FoodService : IFoodService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private const string OpenFoodFactsApiUrl = "https://world.openfoodfacts.org/cgi/search.pl";

    public FoodService(AppDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        // User-Agent is required by OpenFoodFacts or they will block the request
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "MyFitApp/1.0 (student-project)");
    }

    public async Task<List<FoodSearchResult>> SearchFoodItemsAsync(string query, int maxResults = 20)
    {
        try
        {
            var url = $"{OpenFoodFactsApiUrl}?search_terms={Uri.EscapeDataString(query)}&search_simple=1&json=1&page_size={maxResults}";
            
            // OPTIMIZATION: Read headers only first, then stream the content
            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var openFoodResponse = await JsonSerializer.DeserializeAsync<OpenFoodFactsResponse>(stream, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (openFoodResponse?.Products == null)
                return new List<FoodSearchResult>();

            return openFoodResponse.Products
                .Where(p => !string.IsNullOrEmpty(p.Code) && !string.IsNullOrEmpty(p.ProductName))
                .Select(p => new FoodSearchResult
                {
                    ExternalId = p.Code!,
                    Name = p.ProductName ?? "Unknown",
                    Brand = p.Brands,
                    // Map nutritional data - use EnergyKcal100g first, fallback to Energy100g
                    Calories = GetCalories(p.Nutriments),
                    Protein = ParseDecimal(p.Nutriments?.Proteins100g),
                    Carbs = ParseDecimal(p.Nutriments?.Carbohydrates100g),
                    Fats = ParseDecimal(p.Nutriments?.Fat100g),
                    ServingSize = 100,
                    ImageUrl = p.ImageUrl
                })
                .Take(maxResults)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching OpenFoodFacts: {ex.Message}");
            return new List<FoodSearchResult>();
        }
    }

    public async Task<Guid> GetOrCreateLocalFoodItemAsync(string externalId, string externalSource)
    {
        // 1. Check Local Cache
        var existingFoodItem = await _context.FoodItems
            .FirstOrDefaultAsync(f => f.ExternalId == externalId && f.ExternalSource == externalSource);

        if (existingFoodItem != null)
        {
            existingFoodItem.TimesLogged++;
            existingFoodItem.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existingFoodItem.Id;
        }

        // 2. Fetch from External API (Case-Insensitive check)
        if (string.Equals(externalSource, "OpenFoodFacts", StringComparison.OrdinalIgnoreCase))
        {
            var foodData = await FetchFromOpenFoodFactsAsync(externalId);
            
            if (foodData == null)
                throw new Exception($"Could not fetch food item from OpenFoodFacts: {externalId}");

            var newFoodItem = new FoodItem
            {
                Id = Guid.NewGuid(),
                ExternalId = externalId,
                ExternalSource = "OpenFoodFacts",
                Name = foodData.Name,
                Brand = foodData.Brand,
                ServingSize = foodData.ServingSize,
                Calories = foodData.Calories,
                Protein = foodData.Protein,
                Carbs = foodData.Carbs,
                Fats = foodData.Fats,
                ImageUrl = foodData.ImageUrl,
                TimesLogged = 1,
                LastUsedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.FoodItems.Add(newFoodItem);
            await _context.SaveChangesAsync();

            return newFoodItem.Id;
        }

        throw new NotSupportedException($"External source '{externalSource}' is not supported");
    }

    private async Task<FoodSearchResult?> FetchFromOpenFoodFactsAsync(string productCode)
    {
        try
        {
            var url = $"https://world.openfoodfacts.org/api/v0/product/{productCode}.json";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var productResponse = await JsonSerializer.DeserializeAsync<OpenFoodFactsProductResponse>(stream,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (productResponse?.Product == null) return null;

            var p = productResponse.Product;
            return new FoodSearchResult
            {
                ExternalId = productCode,
                Name = p.ProductName ?? "Unknown",
                Brand = p.Brands,
                Calories = GetCalories(p.Nutriments),
                Protein = ParseDecimal(p.Nutriments?.Proteins100g),
                Carbs = ParseDecimal(p.Nutriments?.Carbohydrates100g),
                Fats = ParseDecimal(p.Nutriments?.Fat100g),
                ServingSize = 100,
                ImageUrl = p.ImageUrl
            };
        }
        catch
        {
            return null;
        }
    }

    private static decimal GetCalories(OpenFoodFactsNutriments? nutriments)
    {
        if (nutriments == null) return 0;
        
        // Try energy-kcal_100g first (most accurate)
        var kcal = ParseDecimal(nutriments.EnergyKcal100g);
        if (kcal > 0) return kcal;
        
        // Fallback to energy_100g (might be in kJ, convert to kcal)
        var energy = ParseDecimal(nutriments.Energy100g);
        if (energy > 0)
        {
            // If value is very high, it's likely kJ - convert to kcal (divide by 4.184)
            if (energy > 400) return Math.Round(energy / 4.184m, 1);
            return energy;
        }
        
        return 0;
    }

    private static decimal ParseDecimal(object? value)
    {
        if (value == null) return 0;
        if (value is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Number) return jsonElement.GetDecimal();
            if (jsonElement.ValueKind == JsonValueKind.String) 
                return decimal.TryParse(jsonElement.GetString(), out var result) ? result : 0;
        }
        return decimal.TryParse(value.ToString(), out var parsed) ? parsed : 0;
    }
}

// --- JSON DTOs ---

public class OpenFoodFactsResponse
{
    [JsonPropertyName("products")]
    public List<OpenFoodFactsProduct>? Products { get; set; }
}

public class OpenFoodFactsProductResponse
{
    [JsonPropertyName("product")]
    public OpenFoodFactsProduct? Product { get; set; }
}

public class OpenFoodFactsProduct
{
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("product_name")]
    public string? ProductName { get; set; }

    [JsonPropertyName("brands")]
    public string? Brands { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("nutriments")]
    public OpenFoodFactsNutriments? Nutriments { get; set; }
}

public class OpenFoodFactsNutriments
{
    [JsonPropertyName("energy-kcal_100g")]
    public object? EnergyKcal100g { get; set; }

    // Fallback for alternative field name
    [JsonPropertyName("energy_100g")]
    public object? Energy100g { get; set; }

    [JsonPropertyName("proteins_100g")]
    public object? Proteins100g { get; set; }

    [JsonPropertyName("carbohydrates_100g")]
    public object? Carbohydrates100g { get; set; }

    [JsonPropertyName("fat_100g")]
    public object? Fat100g { get; set; }
}