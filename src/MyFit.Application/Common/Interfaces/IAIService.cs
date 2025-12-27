namespace MyFit.Application.Common.Interfaces;

/// <summary>
/// AI Assistant interface - swappable implementation
/// Currently returns mock data, can be swapped for OpenAI later
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Get AI-powered fitness advice
    /// </summary>
    Task<string> GetFitnessAdviceAsync(string userQuery, string? userContext = null);
    
    /// <summary>
    /// Generate a meal plan suggestion
    /// </summary>
    Task<string> GenerateMealPlanAsync(decimal calorieTarget, string dietaryPreferences);
    
    /// <summary>
    /// Analyze workout routine and provide recommendations
    /// </summary>
    Task<string> AnalyzeWorkoutRoutineAsync(string workoutDescription);
}
