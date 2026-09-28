using Microsoft.EntityFrameworkCore;
using Reservation.Application.Abstractions.Persistence;
using Reservation.Domain.Entities;

namespace Reservation.Infrastructure.Data.Repositories;

public class ProfessionalServiceRepository : IProfessionalServiceRepository
{
    private readonly ReservationDbContext _dbContext;

    public ProfessionalServiceRepository(ReservationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ProfessionalService association, CancellationToken cancellationToken = default)
    {
        await _dbContext.ProfessionalServices.AddAsync(association, cancellationToken);
        // Composite PK: concurrent duplicate pairs → ConflictException → 409.
        await DatabaseConflict.SaveChangesAsync(_dbContext, "The professional already offers this service.", cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid professionalId, Guid serviceId, CancellationToken cancellationToken = default)
        => _dbContext.ProfessionalServices
            .AsNoTracking()
            .AnyAsync(ps => ps.ProfessionalId == professionalId && ps.ServiceId == serviceId, cancellationToken);

    public async Task<IReadOnlyList<ProfessionalService>> GetByProfessionalAsync(Guid professionalId, CancellationToken cancellationToken = default)
        => await _dbContext.ProfessionalServices
            .AsNoTracking()
            .Where(ps => ps.ProfessionalId == professionalId)
            .ToListAsync(cancellationToken);

    public async Task DeleteAsync(Guid professionalId, Guid serviceId, CancellationToken cancellationToken = default)
    {
        var association = await _dbContext.ProfessionalServices
            .FindAsync(new object[] { professionalId, serviceId }, cancellationToken);
        if (association is null)
            return;

        _dbContext.ProfessionalServices.Remove(association);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
