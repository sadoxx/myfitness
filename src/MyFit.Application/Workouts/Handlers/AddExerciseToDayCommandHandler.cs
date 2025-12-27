using MediatR;
using MyFit.Application.Common.Interfaces;
using MyFit.Application.Common.Models;
using MyFit.Application.Workouts.Commands;
using MyFit.Domain.Entities;

namespace MyFit.Application.Workouts.Handlers;

public class AddExerciseToDayCommandHandler : IRequestHandler<AddExerciseToDayCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public AddExerciseToDayCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(AddExerciseToDayCommand request, CancellationToken cancellationToken)
    {
        var workoutExercise = new WorkoutExercise
        {
            Id = Guid.NewGuid(),
            WorkoutDayId = request.WorkoutDayId,
            ExerciseId = request.ExerciseId,
            Sets = request.Sets,
            Reps = request.Reps,
            Weight = request.Weight,
            RestTime = request.RestTime,
            OrderIndex = 0, // Should be calculated based on existing exercises
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkoutExercises.Add(workoutExercise);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(workoutExercise.Id);
    }
}
