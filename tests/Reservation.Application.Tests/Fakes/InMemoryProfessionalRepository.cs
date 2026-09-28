using Reservation.Application.Abstractions.Persistence;
using Reservation.Domain.Entities;

namespace Reservation.Application.Tests.Fakes;

public class InMemoryProfessionalRepository : IProfessionalRepository
{
    public List<Professional> Items { get; } = new();

    public Task AddAsync(Professional professional, CancellationToken cancellationToken = default)
    {
        Items.Add(professional);
        return Task.CompletedTask;
    }

    public Task<Professional?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<Professional>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Professional>>(Items.ToList());
}
