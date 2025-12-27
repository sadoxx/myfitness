using MediatR;
using MyFit.Application.Common.Interfaces;
using MyFit.Application.Common.Models;
using MyFit.Application.Workouts.Commands;
using MyFit.Domain.Entities;

namespace MyFit.Application.Workouts.Handlers;

public class CreateWorkoutPlanCommandHandler : IRequestHandler<CreateWorkoutPlanCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateWorkoutPlanCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateWorkoutPlanCommand request, CancellationToken cancellationToken)
    {
        var workoutPlan = new WorkoutPlan
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name,
            Description = request.Description,
            StartDate = request.StartDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.WorkoutPlans.Add(workoutPlan);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(workoutPlan.Id);
    }
}
