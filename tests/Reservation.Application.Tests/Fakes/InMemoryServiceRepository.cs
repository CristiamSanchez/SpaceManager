using Reservation.Application.Abstractions.Persistence;
using Reservation.Domain.Entities;

namespace Reservation.Application.Tests.Fakes;

public class InMemoryServiceRepository : IServiceRepository
{
    public List<Service> Items { get; } = new();

    public Task AddAsync(Service service, CancellationToken cancellationToken = default)
    {
        Items.Add(service);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Service service, CancellationToken cancellationToken = default) =>
        Task.CompletedTask; // The in-memory list already holds the mutated instance.

    public Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.FirstOrDefault(s => s.Id == id));

    public Task<IReadOnlyList<Service>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Service>>(Items.ToList());
}
