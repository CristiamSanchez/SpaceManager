namespace Reservation.Domain.Entities;

/// <summary>
/// Association between a professional and a service (N:M). See docs/domain-model.md §6.
/// Uniqueness of the pair is a persistence concern and is configured in Infrastructure.
/// </summary>
public class ProfessionalService
{
    public Guid ProfessionalId { get; private set; }
    public Guid ServiceId { get; private set; }

    public ProfessionalService(Guid professionalId, Guid serviceId)
    {
        if (professionalId == Guid.Empty)
            throw new ArgumentException("ProfessionalId must be a valid identifier.", nameof(professionalId));
        if (serviceId == Guid.Empty)
            throw new ArgumentException("ServiceId must be a valid identifier.", nameof(serviceId));

        ProfessionalId = professionalId;
        ServiceId = serviceId;
    }

    // Private parameterless constructor for entity reconstitution.
    private ProfessionalService() { }
}
