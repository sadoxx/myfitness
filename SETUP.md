# MyFit - Setup & Run Instructions

## Prerequisites
- .NET 8 SDK
- PostgreSQL 14+ installed and running
- Visual Studio 2022 or VS Code with C# extension

## Database Setup

### 1. Install PostgreSQL
Download and install PostgreSQL from [postgresql.org](https://www.postgresql.org/download/)

### 2. Create Database
```bash
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE myfit_db;

# Exit psql
\q
```

### 3. Update Connection String
Edit `src/MyFit.API/appsettings.json` and update:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=myfit_db;Username=postgres;Password=YOUR_PASSWORD"
}
```

## Build and Run

### Option 1: Using Visual Studio
1. Open `MyFit.sln`
2. Set `MyFit.API` as startup project
3. Run migrations:
   - Open Package Manager Console
   - Set Default Project to `MyFit.Infrastructure`
   - Run: `Add-Migration InitialCreate`
   - Run: `Update-Database`
4. Press F5 to run

### Option 2: Using Command Line

#### Step 1: Restore packages
```powershell
cd c:\Users\hp\Desktop\myfitness
dotnet restore
```

#### Step 2: Create and apply migrations
```powershell
# Navigate to API project
cd src\MyFit.API

# Install EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create migration
dotnet ef migrations add InitialCreate --project ..\MyFit.Infrastructure --startup-project .

# Apply migration
dotnet ef database update --project ..\MyFit.Infrastructure --startup-project .
```

#### Step 3: Run API
```powershell
# From src/MyFit.API directory
dotnet run
```
The API will start at: https://localhost:7001
Swagger UI: https://localhost:7001/swagger

#### Step 4: Run Blazor Client (in new terminal)
```powershell
cd c:\Users\hp\Desktop\myfitness\src\MyFit.Client
dotnet run
```
The client will start at: https://localhost:5001

## Verify Setup

### 1. Check Database Seeding
The application automatically seeds 20 exercises on first run. Check your database:
```sql
SELECT COUNT(*) FROM "Exercises";
-- Should return 20
```

### 2. Test API
Navigate to https://localhost:7001/swagger and test:
- POST /api/auth/register - Create test account
- POST /api/auth/login - Get JWT token
- GET /api/workouts/exercises - Verify seeded data

### 3. Test Blazor Client
1. Navigate to https://localhost:5001
2. Click "Sign Up" to create account
3. Complete onboarding wizard
4. View dashboard with nutrition charts

## Project Structure

```
MyFit/
├── src/
│   ├── MyFit.Domain/          # Core entities & enums
│   ├── MyFit.Application/     # CQRS commands & queries
│   ├── MyFit.Infrastructure/  # EF Core, services, data access
│   ├── MyFit.API/            # REST API controllers
│   └── MyFit.Client/         # Blazor WASM frontend
└── MyFit.sln
```

## Key Features Implemented

### ✅ Authentication
- JWT-based authentication
- Registration with validation
- Login with token generation

### ✅ Onboarding Wizard
- Multi-step form for user profile
- Automatic BMR/TDEE calculation using Mifflin-St Jeor equation
- Goal-based macro calculations

### ✅ Dashboard
- Three donut charts (Protein, Carbs, Fats) using MudBlazor
- Real-time updates via StateContainer
- Water intake tracking with quick add button
- Sleep tracking display
- Calories remaining counter

### ✅ Nutrition Module - Hybrid API Strategy
Key implementation in `FoodService.cs`:
1. Search OpenFoodFacts API
2. Cache results in local `FoodItems` table by ExternalId
3. Avoid duplicate API calls for frequently used foods
4. Track usage statistics

### ✅ Workout Module
- Seeded exercise database (20 exercises from JSON)
- Exercise filtering by muscle group and difficulty
- Workout plan creation
- Weekly planner structure

### ✅ AI Assistant
- Mock implementation with hardcoded responses
- Swappable interface (`IAIService`)
- Easy to replace with OpenAI GPT implementation

## Architecture Highlights

### Clean Architecture Layers
1. **Domain** - Pure business entities, no dependencies
2. **Application** - CQRS with MediatR, FluentValidation
3. **Infrastructure** - EF Core, external services, JWT
4. **API** - Controllers, middleware, JWT auth
5. **Client** - Blazor WASM, MudBlazor components

### Design Patterns Used
- **CQRS** - Commands for writes, queries for reads
- **Repository Pattern** - Via IApplicationDbContext
- **Dependency Injection** - Throughout all layers
- **Mediator Pattern** - MediatR for decoupled handlers
- **Strategy Pattern** - Hybrid API caching strategy

## Troubleshooting

### Database Connection Issues
```powershell
# Test PostgreSQL connection
psql -U postgres -d myfit_db -c "SELECT version();"
```

### Migration Issues
```powershell
# Drop and recreate database
dotnet ef database drop --force --project ..\MyFit.Infrastructure --startup-project .
dotnet ef database update --project ..\MyFit.Infrastructure --startup-project .
```

### CORS Issues
Ensure API is running on port 7001 and Client on 5001, or update CORS policy in `Program.cs`

## Next Steps

### To Integrate Real OpenAI
1. Install `Betalgo.OpenAI` package in Infrastructure
2. Create `OpenAIService : IAIService`
3. Update DI registration in `Infrastructure/DependencyInjection.cs`

### To Add More Features
- Complete the onboarding wizard UI
- Add nutrition page with food search
- Add workout planner weekly view
- Add progress tracking with charts
- Add user settings page

## API Endpoints

### Auth
- POST /api/auth/register
- POST /api/auth/login
- POST /api/auth/onboarding

### Nutrition
- GET /api/nutrition/search?query=oats
- POST /api/nutrition/meal-log
- POST /api/nutrition/water
- GET /api/nutrition/daily-summary

### Workouts
- GET /api/workouts/exercises
- POST /api/workouts/workout-plan
- POST /api/workouts/workout-day
- POST /api/workouts/workout-exercise
- GET /api/workouts/workout-plan

### Dashboard
- GET /api/dashboard/summary
- POST /api/dashboard/ai-chat

## License
MIT License - Feel free to use for learning and commercial projects.
