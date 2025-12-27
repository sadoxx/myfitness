using MediatR;
using MyFit.Application.Common.Interfaces;
using MyFit.Application.Common.Models;

namespace MyFit.Application.Nutrition.Queries;

/// <summary>
/// Query to search food items using the Hybrid API strategy
/// </summary>
public record SearchFoodItemsQuery : IRequest<Result<List<FoodSearchResult>>>
{
    public string Query { get; init; } = string.Empty;
    public int MaxResults { get; init; } = 20;
}
