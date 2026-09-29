using Microsoft.EntityFrameworkCore;
using Reservation.Application.Abstractions.Persistence;
using Reservation.Domain.Entities;

namespace Reservation.Infrastructure.Data.Repositories;

public class ServiceRepository : IServiceRepository
{
    private readonly ReservationDbContext _dbContext;

    public ServiceRepository(ReservationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Service service, CancellationToken cancellationToken = default)
    {
        await _dbContext.Services.AddAsync(service, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Service service, CancellationToken cancellationToken = default)
    {
        // GetByIdAsync uses AsNoTracking, so the entity arrives detached.
        _dbContext.Services.Update(service);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Services
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
}
