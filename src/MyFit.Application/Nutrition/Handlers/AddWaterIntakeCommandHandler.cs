using MediatR;
using MyFit.Application.Common.Interfaces;
using MyFit.Application.Common.Models;
using MyFit.Application.Nutrition.Commands;
using MyFit.Domain.Entities;

namespace MyFit.Application.Nutrition.Handlers;

public class AddWaterIntakeCommandHandler : IRequestHandler<AddWaterIntakeCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public AddWaterIntakeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(AddWaterIntakeCommand request, CancellationToken cancellationToken)
    {
        var waterIntake = new WaterIntake
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Date = (request.Date ?? DateTime.UtcNow).Date,
            Amount = request.Amount,
            LoggedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.WaterIntakes.Add(waterIntake);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(waterIntake.Id);
    }
}
