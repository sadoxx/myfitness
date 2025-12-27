using MyFit.Application.Common.Interfaces;

namespace MyFit.Infrastructure.Services;

/// <summary>
/// Mock AI Service - returns hardcoded fitness advice
/// Can be swapped for OpenAI implementation later
/// </summary>
public class MockAIService : IAIService
{
    public Task<string> GetFitnessAdviceAsync(string userQuery, string? userContext = null)
    {
        var responses = new[]
        {
            "Great question! Remember to stay hydrated and maintain a balanced diet with adequate protein intake.",
            "For optimal results, focus on progressive overload in your workouts and ensure you're getting 7-9 hours of sleep.",
            "Consider tracking your macronutrients to ensure you're meeting your daily protein, carbs, and fat goals.",
            "Don't forget to include rest days in your routine. Recovery is just as important as training!",
            "For muscle gain, aim for a slight calorie surplus (200-300 calories) above your TDEE."
        };

        var random = new Random();
        var response = responses[random.Next(responses.Length)];
        
        return Task.FromResult($"💡 {response}\n\n_Note: This is mock AI advice. Real OpenAI integration coming soon!_");
    }

    public Task<string> GenerateMealPlanAsync(decimal calorieTarget, string dietaryPreferences)
    {
        return Task.FromResult($@"**Sample Meal Plan for {calorieTarget} calories:**

**Breakfast (25%)**
- Oatmeal with berries and protein powder
- Greek yogurt
- Black coffee

**Lunch (30%)**
- Grilled chicken breast
- Brown rice
- Mixed vegetables
- Olive oil dressing

**Dinner (30%)**
- Salmon fillet
- Sweet potato
- Steamed broccoli

**Snacks (15%)**
- Almonds
- Apple with peanut butter

_This is a mock meal plan. For personalized nutrition advice, consult a registered dietitian._");
    }

    public Task<string> AnalyzeWorkoutRoutineAsync(string workoutDescription)
    {
        return Task.FromResult(@"**Workout Analysis:**

✅ **Strengths:**
- Good exercise selection
- Balanced muscle group coverage

💡 **Recommendations:**
- Ensure progressive overload each week
- Include adequate rest between sets (60-90 seconds)
- Consider adding core exercises
- Track your weights to monitor progress

_This is mock AI analysis. Full AI integration coming soon!_");
    }
}
