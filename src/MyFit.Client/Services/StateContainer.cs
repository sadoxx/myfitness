namespace MyFit.Client.Services;

/// <summary>
/// State container for real-time dashboard updates
/// </summary>
public class StateContainer
{
    private DailyNutritionData? _nutritionData;

    public DailyNutritionData? NutritionData
    {
        get => _nutritionData;
        set
        {
            _nutritionData = value;
            NotifyStateChanged();
        }
    }

    public event Action? OnChange;

    private void NotifyStateChanged() => OnChange?.Invoke();
}

public class DailyNutritionData
{
    public decimal TotalCalories { get; set; }
    public decimal TotalProtein { get; set; }
    public decimal TotalCarbs { get; set; }
    public decimal TotalFats { get; set; }
    public decimal CalorieGoal { get; set; }
    public decimal ProteinGoal { get; set; }
    public decimal CarbsGoal { get; set; }
    public decimal FatsGoal { get; set; }
    public decimal TotalWater { get; set; }
    public decimal WaterGoal { get; set; }
    public decimal CaloriesRemaining { get; set; }
}
