using Microsoft.EntityFrameworkCore;
using Reservation.Application.Abstractions.Persistence;
using Reservation.Domain.Entities;

namespace Reservation.Infrastructure.Data.Repositories;

public class ProfessionalRepository : IProfessionalRepository
{
    private readonly ReservationDbContext _dbContext;

    public ProfessionalRepository(ReservationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Professional professional, CancellationToken cancellationToken = default)
    {
        await _dbContext.Professionals.AddAsync(professional, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Professional professional, CancellationToken cancellationToken = default)
    {
        // GetByIdAsync uses AsNoTracking, so the entity arrives detached.
        _dbContext.Professionals.Update(professional);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Professional?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Professionals
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Professional>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Professionals
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
}
