using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyFit.Domain.Entities;
using MyFit.Domain.Enums;
using MyFit.Infrastructure.Data;
using System.Text.Json;

namespace MyFit.Infrastructure.Seeds;

/// <summary>
/// Database initializer that seeds exercises on startup if database is empty
/// </summary>
public class DbInitializer
{
    private readonly AppDbContext _context;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(AppDbContext context, ILogger<DbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Ensure database is created
            await _context.Database.MigrateAsync();

            // Check if exercises already exist
            if (await _context.Exercises.AnyAsync())
            {
                _logger.LogInformation("Database already contains exercises. Skipping seed.");
                return;
            }

            _logger.LogInformation("Seeding exercises from exercises.json...");

            // Read exercises.json
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Seeds", "exercises.json");
            
            if (!File.Exists(jsonPath))
            {
                _logger.LogWarning($"exercises.json not found at {jsonPath}");
                return;
            }

            var jsonString = await File.ReadAllTextAsync(jsonPath);
            var exerciseDtos = JsonSerializer.Deserialize<List<ExerciseSeedDto>>(jsonString);

            if (exerciseDtos == null || !exerciseDtos.Any())
            {
                _logger.LogWarning("No exercises found in exercises.json");
                return;
            }

            // Convert to entities
            var exercises = exerciseDtos.Select(dto => new Exercise
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                MuscleGroup = (MuscleGroup)dto.MuscleGroup,
                Difficulty = (Difficulty)dto.Difficulty,
                Instructions = dto.Instructions,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            // Add to database
            await _context.Exercises.AddRangeAsync(exercises);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Successfully seeded {exercises.Count} exercises");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}

public class ExerciseSeedDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MuscleGroup { get; set; }
    public int Difficulty { get; set; }
    public string? Instructions { get; set; }
}
