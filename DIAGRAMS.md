# MyFit System Diagrams

## Class Diagram

```plantuml
@startuml MyFit Class Diagram

!define ENTITY_COLOR #1e293b
!define ENUM_COLOR #334155

skinparam backgroundColor #0f172a
skinparam class {
    BackgroundColor ENTITY_COLOR
    BorderColor #667eea
    ArrowColor #667eea
    FontColor #f8fafc
}

' Core Entities
class ApplicationUser {
    +Guid Id
    +string UserName
    +string Email
    +DateTime CreatedAt
    --
    +UserProfile Profile
    +ICollection<MealLog> MealLogs
    +ICollection<WorkoutLog> WorkoutLogs
    +ICollection<WeightLog> WeightLogs
}

class UserProfile {
    +Guid Id
    +Guid UserId
    +Gender Gender
    +int Age
    +decimal Weight
    +decimal Height
    +FitnessGoal Goal
    +ActivityLevel ActivityLevel
    +decimal DailyCalorieGoal
    +decimal DailyProteinGoal
    +decimal DailyCarbsGoal
    +decimal DailyFatsGoal
    +bool IsOnboardingComplete
    --
    +ApplicationUser User
}

' Nutrition Entities
class FoodItem {
    +Guid Id
    +string Name
    +string Brand
    +decimal ServingSize
    +string ServingUnit
    +decimal Calories
    +decimal Protein
    +decimal Carbs
    +decimal Fats
    +string Barcode
    +bool IsCustom
    --
    +ICollection<MealLog> MealLogs
}

class MealLog {
    +Guid Id
    +Guid UserId
    +Guid FoodItemId
    +DateTime LogDate
    +MealType MealType
    +decimal Quantity
    +string QuantityUnit
    +decimal TotalCalories
    +decimal TotalProtein
    +decimal TotalCarbs
    +decimal TotalFats
    +string Notes
    --
    +ApplicationUser User
    +FoodItem FoodItem
}

' Workout Entities
class Exercise {
    +Guid Id
    +string Name
    +ExerciseCategory Category
    +MuscleGroup PrimaryMuscle
    +MuscleGroup SecondaryMuscle
    +string Description
    +string VideoUrl
    +bool IsCustom
    --
    +ICollection<ExerciseLog> ExerciseLogs
}

class WorkoutLog {
    +Guid Id
    +Guid UserId
    +DateTime WorkoutDate
    +string WorkoutName
    +int DurationMinutes
    +string Notes
    --
    +ApplicationUser User
    +ICollection<ExerciseLog> ExerciseLogs
}

class ExerciseLog {
    +Guid Id
    +Guid WorkoutLogId
    +Guid ExerciseId
    +int OrderIndex
    --
    +WorkoutLog WorkoutLog
    +Exercise Exercise
    +ICollection<SetLog> SetLogs
}

class SetLog {
    +Guid Id
    +Guid ExerciseLogId
    +int SetNumber
    +int Reps
    +decimal Weight
    +bool Completed
    --
    +ExerciseLog ExerciseLog
}

' Progress Tracking
class WeightLog {
    +Guid Id
    +Guid UserId
    +decimal Weight
    +DateTime LogDate
    +string Notes
    --
    +ApplicationUser User
}

class WaterIntake {
    +Guid Id
    +Guid UserId
    +DateTime LogDate
    +int Glasses
    +decimal MillilitersPerGlass
    --
    +ApplicationUser User
}

class SleepLog {
    +Guid Id
    +Guid UserId
    +DateTime SleepDate
    +decimal Hours
    +int Quality
    +string Notes
    --
    +ApplicationUser User
}

' Enums
enum Gender <<ENUM_COLOR>> {
    Male
    Female
}

enum FitnessGoal <<ENUM_COLOR>> {
    WeightLoss
    MuscleGain
    Maintenance
}

enum ActivityLevel <<ENUM_COLOR>> {
    Sedentary
    LightlyActive
    ModeratelyActive
    VeryActive
    ExtraActive
}

enum MealType <<ENUM_COLOR>> {
    Breakfast
    Lunch
    Dinner
    Snack
}

enum ExerciseCategory <<ENUM_COLOR>> {
    Strength
    Cardio
    Flexibility
    Sports
}

enum MuscleGroup <<ENUM_COLOR>> {
    Chest
    Back
    Shoulders
    Arms
    Legs
    Core
    FullBody
}

' Relationships
ApplicationUser "1" -- "1" UserProfile : has >
ApplicationUser "1" -- "*" MealLog : logs >
ApplicationUser "1" -- "*" WorkoutLog : performs >
ApplicationUser "1" -- "*" WeightLog : tracks >
ApplicationUser "1" -- "*" WaterIntake : records >
ApplicationUser "1" -- "*" SleepLog : records >

FoodItem "1" -- "*" MealLog : consumed in >
Exercise "1" -- "*" ExerciseLog : performed as >

WorkoutLog "1" -- "*" ExerciseLog : contains >
ExerciseLog "1" -- "*" SetLog : consists of >

UserProfile -- Gender
UserProfile -- FitnessGoal
UserProfile -- ActivityLevel
MealLog -- MealType
Exercise -- ExerciseCategory
Exercise -- MuscleGroup

@enduml
```

## Sequence Diagram - User Registration & Meal Logging

```plantuml
@startuml MyFit Sequence Diagram

!define USER_COLOR #667eea
!define CLIENT_COLOR #10b981
!define API_COLOR #3b82f6
!define DB_COLOR #f59e0b

skinparam backgroundColor #0f172a
skinparam sequence {
    ArrowColor #667eea
    ActorBorderColor #667eea
    ActorBackgroundColor #1e293b
    ActorFontColor #f8fafc
    ParticipantBorderColor #667eea
    ParticipantBackgroundColor #1e293b
    ParticipantFontColor #f8fafc
    LifeLineBorderColor #475569
    LifeLineBackgroundColor #1e293b
}

actor User as U
participant "Blazor Client" as C
participant "API Controller" as A
participant "MediatR" as M
participant "Database" as D

== User Registration ==
U -> C: Navigate to Register
activate C
C -> C: Display registration form
U -> C: Enter credentials\n(email, password)
C -> A: POST /api/auth/register
activate A
A -> A: Validate input
A -> D: Create ApplicationUser
activate D
D --> A: User created
deactivate D
A -> A: Generate JWT token
A --> C: Return token & user
deactivate A
C -> C: Store token in localStorage
C --> U: Redirect to dashboard
deactivate C

== Profile Setup (Onboarding) ==
U -> C: Navigate to Settings
activate C
C -> A: GET /api/auth/profile
activate A
A -> D: Query UserProfile
activate D
D --> A: Profile data
deactivate D
A --> C: Profile info
deactivate A
C -> C: Display onboarding form
U -> C: Enter profile data\n(age, weight, height, goals)
C -> C: Calculate BMR & TDEE\n(Mifflin-St Jeor equation)
C -> A: POST /api/auth/onboarding
activate A
A -> D: Create/Update UserProfile
activate D
D --> A: Profile saved
deactivate D
A --> C: Success response
deactivate A
C --> U: Show confirmation
deactivate C

== Log Meal ==
U -> C: Navigate to Nutrition
activate C
C -> A: GET /api/nutrition/today
activate A
A -> D: Query today's meals
activate D
D --> A: Meal list
deactivate D
A --> C: Daily nutrition data
deactivate A
C -> C: Display dashboard\nwith progress bars
U -> C: Click "Log Meal"
C -> C: Open meal dialog
U -> C: Search for food\n(e.g., "chicken")
C -> A: GET /api/nutrition/foods/search?q=chicken
activate A
A -> D: Search FoodItems
activate D
D --> A: Matching foods
deactivate D
A --> C: Food results
deactivate A
C -> C: Display food list
U -> C: Select food & quantity
U -> C: Select meal type (Breakfast)
C -> C: Calculate total macros\n(quantity × per-serving values)
C -> A: POST /api/nutrition/log-meal
activate A
A -> M: Send LogMealCommand
activate M
M -> D: Insert MealLog
activate D
D --> M: Meal logged
deactivate D
M --> A: Success
deactivate M
A --> C: Meal logged successfully
deactivate A
C -> A: GET /api/nutrition/today
activate A
A -> D: Query updated meals
activate D
D --> A: Updated meal list
deactivate D
A --> C: Refreshed data
deactivate A
C -> C: Update UI & progress bars
C --> U: Show updated dashboard
deactivate C

== Log Workout ==
U -> C: Navigate to Workouts
activate C
C -> A: GET /api/workouts/exercises
activate A
A -> D: Query exercises
activate D
D --> A: Exercise list
deactivate D
A --> C: Exercises
deactivate A
U -> C: Click "Start Workout"
C -> C: Create new WorkoutLog
U -> C: Add exercise (e.g., Bench Press)
C -> C: Add to exercise list
U -> C: Log sets (4x8 @ 80kg)
loop For each set
    U -> C: Enter reps & weight
    C -> C: Add to set list
end
U -> C: Click "Finish Workout"
C -> A: POST /api/workouts/log
activate A
A -> D: Insert WorkoutLog\n+ ExerciseLogs\n+ SetLogs
activate D
D --> A: Workout saved
deactivate D
A --> C: Success
deactivate A
C --> U: Show confirmation
deactivate C

== Track Progress ==
U -> C: Navigate to Progress
activate C
C -> A: GET /api/progress/weight
activate A
A -> D: Query WeightLogs
activate D
D --> A: Weight history
deactivate D
A --> C: Weight data
deactivate A
C -> A: GET /api/progress/weekly-stats
activate A
A -> D: Aggregate last 7 days
activate D
D --> A: Stats (meals, workouts, calories)
deactivate D
A --> C: Weekly summary
deactivate A
C -> A: GET /api/progress/nutrition
activate A
A -> D: Query daily nutrition
activate D
D --> A: 7-day nutrition data
deactivate D
A --> C: Nutrition progress
deactivate A
C -> C: Display charts\n& statistics
C --> U: Show progress overview
deactivate C

@enduml
```

## Use Case Diagram

```plantuml
@startuml MyFit Use Case Diagram

!define PRIMARY_COLOR #667eea
!define SECONDARY_COLOR #10b981

skinparam backgroundColor #0f172a
skinparam actor {
    BackgroundColor #1e293b
    BorderColor PRIMARY_COLOR
    FontColor #f8fafc
}
skinparam usecase {
    BackgroundColor #1e293b
    BorderColor PRIMARY_COLOR
    FontColor #f8fafc
    ArrowColor #667eea
}
skinparam rectangle {
    BackgroundColor #0f172a
    BorderColor #475569
    FontColor #f8fafc
}

left to right direction

actor "User" as User
actor "System" as System

rectangle "MyFit Application" {
    
    rectangle "Authentication" as Auth {
        usecase "Register Account" as UC1
        usecase "Login" as UC2
        usecase "Logout" as UC3
        usecase "Complete Onboarding" as UC4
    }
    
    rectangle "Nutrition Management" as Nutrition {
        usecase "Search Foods" as UC5
        usecase "Log Meal" as UC6
        usecase "View Daily Nutrition" as UC7
        usecase "Delete Meal Log" as UC8
        usecase "Add Custom Food" as UC9
        usecase "Log Water Intake" as UC10
    }
    
    rectangle "Workout Tracking" as Workout {
        usecase "Browse Exercises" as UC11
        usecase "Start Workout" as UC12
        usecase "Add Exercise to Workout" as UC13
        usecase "Log Sets & Reps" as UC14
        usecase "Finish Workout" as UC15
        usecase "View Workout History" as UC16
        usecase "Add Custom Exercise" as UC17
    }
    
    rectangle "Progress Tracking" as Progress {
        usecase "Log Weight" as UC18
        usecase "View Weight History" as UC19
        usecase "View Weekly Stats" as UC20
        usecase "View Nutrition Progress" as UC21
        usecase "View Workout Progress" as UC22
    }
    
    rectangle "Profile Management" as Profile {
        usecase "Update Personal Info" as UC23
        usecase "Update Fitness Goals" as UC24
        usecase "View Calculated Goals" as UC25
    }
    
    rectangle "Dashboard" as Dashboard {
        usecase "View Dashboard" as UC26
        usecase "View Today's Summary" as UC27
    }
}

' User relationships
User --> UC1
User --> UC2
User --> UC3
User --> UC4

User --> UC5
User --> UC6
User --> UC7
User --> UC8
User --> UC9
User --> UC10

User --> UC11
User --> UC12
User --> UC13
User --> UC14
User --> UC15
User --> UC16
User --> UC17

User --> UC18
User --> UC19
User --> UC20
User --> UC21
User --> UC22

User --> UC23
User --> UC24
User --> UC25

User --> UC26
User --> UC27

' System relationships
UC4 ..> UC25 : <<include>>
UC6 ..> UC5 : <<include>>
UC12 ..> UC11 : <<include>>
UC26 ..> UC7 : <<include>>
UC26 ..> UC27 : <<include>>

System ..> UC25 : calculates BMR/TDEE
System ..> UC7 : aggregates nutrition

' Extensions
UC6 ..> UC9 : <<extend>>
UC13 ..> UC17 : <<extend>>

note right of UC25
  Automatically calculates:
  - BMR (Mifflin-St Jeor)
  - TDEE (Activity multiplier)
  - Macro distribution
end note

note right of UC6
  Calculates total macros:
  quantity × per-serving values
end note

note right of UC20
  Aggregates:
  - Meals logged
  - Workouts completed
  - Avg daily calories
  - Total exercise time
end note

@enduml
```

## Entity Relationship Diagram

```plantuml
@startuml MyFit ERD

!define TABLE_COLOR #1e293b
!define PK_COLOR #667eea
!define FK_COLOR #10b981

skinparam backgroundColor #0f172a
skinparam entity {
    BackgroundColor TABLE_COLOR
    BorderColor #667eea
    FontColor #f8fafc
    AttributeFontColor #cbd5e1
}
skinparam arrow {
    Color #667eea
}

entity "Users" as users {
    *Id : UUID <<PK>>
    --
    UserName : VARCHAR(256)
    Email : VARCHAR(256)
    PasswordHash : TEXT
    CreatedAt : TIMESTAMP
    UpdatedAt : TIMESTAMP
    IsDeleted : BOOLEAN
}

entity "UserProfiles" as profiles {
    *Id : UUID <<PK>>
    *UserId : UUID <<FK>>
    --
    Gender : INT
    Age : INT
    Weight : DECIMAL(5,2)
    Height : DECIMAL(5,2)
    Goal : INT
    ActivityLevel : INT
    DailyCalorieGoal : DECIMAL
    DailyProteinGoal : DECIMAL
    DailyCarbsGoal : DECIMAL
    DailyFatsGoal : DECIMAL
    IsOnboardingComplete : BOOLEAN
    CreatedAt : TIMESTAMP
    UpdatedAt : TIMESTAMP
    IsDeleted : BOOLEAN
}

entity "FoodItems" as foods {
    *Id : UUID <<PK>>
    --
    Name : VARCHAR(200)
    Brand : VARCHAR(200)
    ServingSize : DECIMAL
    ServingUnit : VARCHAR(50)
    Calories : DECIMAL
    Protein : DECIMAL
    Carbs : DECIMAL
    Fats : DECIMAL
    Barcode : VARCHAR(50)
    IsCustom : BOOLEAN
    CreatedAt : TIMESTAMP
    IsDeleted : BOOLEAN
}

entity "MealLogs" as meals {
    *Id : UUID <<PK>>
    *UserId : UUID <<FK>>
    *FoodItemId : UUID <<FK>>
    --
    LogDate : TIMESTAMP
    MealType : INT
    Quantity : DECIMAL
    QuantityUnit : VARCHAR(50)
    TotalCalories : DECIMAL
    TotalProtein : DECIMAL
    TotalCarbs : DECIMAL
    TotalFats : DECIMAL
    Notes : TEXT
    CreatedAt : TIMESTAMP
    UpdatedAt : TIMESTAMP
    IsDeleted : BOOLEAN
}

entity "Exercises" as exercises {
    *Id : UUID <<PK>>
    --
    Name : VARCHAR(200)
    Category : INT
    PrimaryMuscle : INT
    SecondaryMuscle : INT
    Description : TEXT
    VideoUrl : VARCHAR(500)
    IsCustom : BOOLEAN
    CreatedAt : TIMESTAMP
    IsDeleted : BOOLEAN
}

entity "WorkoutLogs" as workouts {
    *Id : UUID <<PK>>
    *UserId : UUID <<FK>>
    --
    WorkoutDate : TIMESTAMP
    WorkoutName : VARCHAR(200)
    DurationMinutes : INT
    Notes : TEXT
    CreatedAt : TIMESTAMP
    UpdatedAt : TIMESTAMP
    IsDeleted : BOOLEAN
}

entity "ExerciseLogs" as exercise_logs {
    *Id : UUID <<PK>>
    *WorkoutLogId : UUID <<FK>>
    *ExerciseId : UUID <<FK>>
    --
    OrderIndex : INT
    CreatedAt : TIMESTAMP
    UpdatedAt : TIMESTAMP
    IsDeleted : BOOLEAN
}

entity "SetLogs" as sets {
    *Id : UUID <<PK>>
    *ExerciseLogId : UUID <<FK>>
    --
    SetNumber : INT
    Reps : INT
    Weight : DECIMAL(6,2)
    Completed : BOOLEAN
    CreatedAt : TIMESTAMP
    UpdatedAt : TIMESTAMP
    IsDeleted : BOOLEAN
}

entity "WeightLogs" as weight {
    *Id : UUID <<PK>>
    *UserId : UUID <<FK>>
    --
    Weight : DECIMAL(5,2)
    LogDate : TIMESTAMP
    Notes : TEXT
    CreatedAt : TIMESTAMP
    UpdatedAt : TIMESTAMP
    IsDeleted : BOOLEAN
}

entity "WaterIntakes" as water {
    *Id : UUID <<PK>>
    *UserId : UUID <<FK>>
    --
    LogDate : TIMESTAMP
    Glasses : INT
    MillilitersPerGlass : DECIMAL
    CreatedAt : TIMESTAMP
    IsDeleted : BOOLEAN
}

entity "SleepLogs" as sleep {
    *Id : UUID <<PK>>
    *UserId : UUID <<FK>>
    --
    SleepDate : TIMESTAMP
    Hours : DECIMAL(3,1)
    Quality : INT
    Notes : TEXT
    CreatedAt : TIMESTAMP
    IsDeleted : BOOLEAN
}

' Relationships
users ||--|| profiles : "has one"
users ||--o{ meals : "logs"
users ||--o{ workouts : "performs"
users ||--o{ weight : "tracks"
users ||--o{ water : "records"
users ||--o{ sleep : "records"

foods ||--o{ meals : "consumed in"
exercises ||--o{ exercise_logs : "performed as"

workouts ||--o{ exercise_logs : "contains"
exercise_logs ||--o{ sets : "consists of"

@enduml
```

## Architecture Diagram

```plantuml
@startuml MyFit Architecture

!define COMPONENT_COLOR #1e293b
!define DATABASE_COLOR #0f4c81
!define EXTERNAL_COLOR #475569

skinparam backgroundColor #0f172a
skinparam component {
    BackgroundColor COMPONENT_COLOR
    BorderColor #667eea
    FontColor #f8fafc
    ArrowColor #667eea
}
skinparam database {
    BackgroundColor DATABASE_COLOR
    BorderColor #3b82f6
    FontColor #f8fafc
}
skinparam cloud {
    BackgroundColor EXTERNAL_COLOR
    BorderColor #94a3b8
    FontColor #f8fafc
}

package "Client Layer" {
    [Blazor WebAssembly] as blazor
    component "Pages" {
        [Dashboard]
        [Nutrition]
        [Workouts]
        [Progress]
        [Settings]
    }
    component "Services" {
        [ApiService]
        [StateContainer]
    }
    component "Shared Components" {
        [MainLayout]
        [NavMenu]
        [Dialogs]
    }
}

package "API Layer" {
    [ASP.NET Core API] as api
    component "Controllers" {
        [AuthController]
        [NutritionController]
        [WorkoutsController]
        [ProgressController]
        [DashboardController]
    }
    component "Middleware" {
        [GlobalExceptionHandler]
        [Authentication]
    }
}

package "Application Layer" {
    [MediatR] as mediator
    component "Commands & Queries" {
        [Auth Commands]
        [Nutrition Commands]
        [Workout Commands]
    }
    component "Handlers" {
        [Command Handlers]
        [Query Handlers]
    }
    component "Behaviors" {
        [Validation Behavior]
        [Logging Behavior]
    }
}

package "Domain Layer" {
    component "Entities" {
        [ApplicationUser]
        [UserProfile]
        [MealLog]
        [WorkoutLog]
        [FoodItem]
        [Exercise]
    }
    component "Enums" {
        [Gender]
        [FitnessGoal]
        [ActivityLevel]
        [MealType]
    }
}

package "Infrastructure Layer" {
    [Entity Framework Core] as ef
    component "Data" {
        [AppDbContext]
        [Configurations]
    }
    component "Services" {
        [TokenService]
        [FoodService]
    }
    database "PostgreSQL" as db {
        [Users]
        [UserProfiles]
        [MealLogs]
        [WorkoutLogs]
        [FoodItems]
        [Exercises]
        [WeightLogs]
    }
}

cloud "External Services" {
    [JWT Authentication] as jwt
}

' Relationships
blazor --> api : "HTTP/JSON"
[ApiService] --> api : "REST API"
api --> [Middleware]
[Controllers] --> mediator : "Send Commands"
mediator --> [Handlers]
[Handlers] --> ef : "Query/Persist"
ef --> db : "ORM"
[TokenService] --> jwt
api ..> jwt : "Validates"

note right of blazor
  SPA hosted on Nginx
  Port: 8080
end note

note right of api
  REST API
  Port: 5000
  JWT Authentication
end note

note right of db
  PostgreSQL 16
  Port: 5432
  Docker Container
end note

@enduml
```

## How to Use These Diagrams

1. **Online PlantUML Editor**: 
   - Visit: https://www.plantuml.com/plantuml/uml/
   - Copy and paste any diagram code
   - Click "Submit" to generate

2. **VS Code Extension**:
   ```bash
   # Install PlantUML extension
   code --install-extension jebbs.plantuml
   ```
   - Create a `.puml` file
   - Paste the diagram code
   - Press `Alt+D` to preview

3. **PlantUML CLI** (requires Java):
   ```bash
   # Install PlantUML
   choco install plantuml
   
   # Generate PNG
   plantuml diagram.puml
   ```

4. **Markdown Preview**:
   - Many markdown viewers support PlantUML
   - Use code blocks with ```plantuml
