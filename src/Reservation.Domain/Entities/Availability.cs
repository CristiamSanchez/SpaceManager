namespace Reservation.Domain.Entities;

/// <summary>
/// Period of time in which a professional can receive reservations. See docs/domain-model.md §7.
/// Overlap detection requires other records and is handled in later phases.
/// </summary>
public class Availability
{
    public Guid Id { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public bool IsActive { get; private set; }

    public Availability(Guid professionalId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (professionalId == Guid.Empty)
            throw new ArgumentException("ProfessionalId must be a valid identifier.", nameof(professionalId));
        if (endTime <= startTime)
            throw new ArgumentException("StartTime must be earlier than EndTime.", nameof(endTime));

        Id = Guid.NewGuid();
        ProfessionalId = professionalId;
        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        IsActive = true;
    }

    // Private parameterless constructor for entity reconstitution.
    private Availability() { }

    /// <summary>Changes the schedule of this period. The professional cannot be changed.</summary>
    public void Update(DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            throw new ArgumentException("StartTime must be earlier than EndTime.", nameof(endTime));

        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
