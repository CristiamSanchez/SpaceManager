using Reservation.Application.Abstractions.Persistence;
using Reservation.Domain.Entities;

namespace Reservation.Application.Tests.Fakes;

public class InMemoryProfessionalServiceRepository : IProfessionalServiceRepository
{
    public List<ProfessionalService> Items { get; } = new();

    public Task AddAsync(ProfessionalService professionalService, CancellationToken cancellationToken = default)
    {
        Items.Add(professionalService);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(Guid professionalId, Guid serviceId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.Any(ps => ps.ProfessionalId == professionalId && ps.ServiceId == serviceId));

    public Task<IReadOnlyList<ProfessionalService>> GetByProfessionalAsync(Guid professionalId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ProfessionalService>>(Items.Where(ps => ps.ProfessionalId == professionalId).ToList());

    public Task DeleteAsync(Guid professionalId, Guid serviceId, CancellationToken cancellationToken = default)
    {
        Items.RemoveAll(ps => ps.ProfessionalId == professionalId && ps.ServiceId == serviceId);
        return Task.CompletedTask;
    }
}
