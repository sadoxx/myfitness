# MyFit - Comprehensive Fitness & Nutrition Web Application

## 🏗️ Solution Structure

```
MyFit/
│
├── src/
│   ├── MyFit.Domain/                    # Core Domain Layer
│   │   ├── Entities/                    # Domain Entities
│   │   │   ├── ApplicationUser.cs
│   │   │   ├── UserProfile.cs
│   │   │   ├── Exercise.cs
│   │   │   ├── WorkoutPlan.cs
│   │   │   ├── WorkoutDay.cs
│   │   │   ├── WorkoutExercise.cs
│   │   │   ├── FoodItem.cs
│   │   │   ├── MealLog.cs
│   │   │   ├── WaterIntake.cs
│   │   │   └── SleepLog.cs
│   │   ├── Enums/                       # Domain Enumerations
│   │   │   ├── Gender.cs
│   │   │   ├── ActivityLevel.cs
│   │   │   ├── FitnessGoal.cs
│   │   │   ├── MuscleGroup.cs
│   │   │   ├── Difficulty.cs
│   │   │   └── MealType.cs
│   │   └── Common/                      # Base Entities
│   │       └── BaseEntity.cs
│   │
│   ├── MyFit.Application/               # Application Layer (CQRS with MediatR)
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IApplicationDbContext.cs
│   │   │   │   ├── IFoodService.cs
│   │   │   │   ├── IAIService.cs
│   │   │   │   └── ITokenService.cs
│   │   │   ├── Models/
│   │   │   │   ├── Result.cs
│   │   │   │   └── PaginatedList.cs
│   │   │   └── Behaviors/
│   │   │       └── ValidationBehavior.cs
│   │   ├── Auth/
│   │   │   ├── Commands/
│   │   │   │   ├── RegisterCommand.cs
│   │   │   │   ├── LoginCommand.cs
│   │   │   │   └── CompleteOnboardingCommand.cs
│   │   │   └── Queries/
│   │   │       └── GetCurrentUserQuery.cs
│   │   ├── Nutrition/
│   │   │   ├── Commands/
│   │   │   │   ├── AddMealLogCommand.cs
│   │   │   │   └── AddWaterIntakeCommand.cs
│   │   │   └── Queries/
│   │   │       ├── GetDailyNutritionQuery.cs
│   │   │       └── SearchFoodItemsQuery.cs
│   │   ├── Workouts/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateWorkoutPlanCommand.cs
│   │   │   │   └── AddExerciseToDayCommand.cs
│   │   │   └── Queries/
│   │   │       ├── GetWorkoutPlanQuery.cs
│   │   │       └── GetAllExercisesQuery.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── MyFit.Infrastructure/            # Infrastructure Layer
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Configurations/          # EF Core Configurations
│   │   │   │   ├── ApplicationUserConfiguration.cs
│   │   │   │   ├── WorkoutPlanConfiguration.cs
│   │   │   │   ├── FoodItemConfiguration.cs
│   │   │   │   └── MealLogConfiguration.cs
│   │   │   └── Migrations/
│   │   ├── Seeds/
│   │   │   ├── exercises.json
│   │   │   └── DbInitializer.cs
│   │   ├── Services/
│   │   │   ├── FoodService.cs           # Hybrid API Strategy
│   │   │   ├── MockAIService.cs
│   │   │   └── TokenService.cs
│   │   ├── Repositories/
│   │   │   └── Repository.cs            # Generic Repository
│   │   └── DependencyInjection.cs
│   │
│   ├── MyFit.API/                       # Web API Layer
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── NutritionController.cs
│   │   │   ├── WorkoutsController.cs
│   │   │   └── DashboardController.cs
│   │   ├── Middleware/
│   │   │   └── GlobalExceptionHandlerMiddleware.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   └── MyFit.Client/                    # Blazor WASM Frontend
│       ├── Pages/
│       │   ├── Index.razor              # Dashboard
│       │   ├── Auth/
│       │   │   ├── Login.razor
│       │   │   ├── Register.razor
│       │   │   └── Onboarding.razor
│       │   ├── Nutrition/
│       │   │   ├── Meals.razor
│       │   │   └── AddMeal.razor
│       │   └── Workouts/
│       │       ├── WorkoutPlanner.razor
│       │       └── ExerciseLibrary.razor
│       ├── Shared/
│       │   ├── MainLayout.razor
│       │   ├── NavMenu.razor
│       │   └── AIAssistant.razor
│       ├── Services/
│       │   ├── ApiService.cs
│       │   └── StateContainer.cs
│       ├── Program.cs
│       └── wwwroot/
│
└── MyFit.sln
```

## 🚀 Technology Stack

- **Framework:** .NET 8
- **Architecture:** Clean Architecture
- **Frontend:** Blazor WebAssembly with MudBlazor
- **Backend:** ASP.NET Core Web API
- **Database:** PostgreSQL with EF Core (Code-First)
- **Authentication:** ASP.NET Core Identity + JWT
- **Patterns:** CQRS (MediatR), Repository Pattern
- **Validation:** FluentValidation

## 📦 Key Features

1. **Authentication & Onboarding** - Multi-step wizard with BMR/TDEE calculation
2. **Dashboard** - Real-time nutrition tracking with donut charts
3. **Workout Module** - Weekly planner with seeded exercise database
4. **Nutrition Module** - Hybrid API strategy (OpenFoodFacts + local cache)
5. **AI Assistant** - Swappable interface for future OpenAI integration

## 🔧 Setup Instructions

1. Install PostgreSQL
2. Update connection string in `appsettings.json`
3. Run migrations: `dotnet ef database update`
4. Start API: `dotnet run --project src/MyFit.API`
5. Start Client: `dotnet run --project src/MyFit.Client`
