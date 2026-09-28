namespace Reservation.Domain.Entities;

/// <summary>
/// Service that can be reserved. See docs/domain-model.md §4.
/// </summary>
public class Service
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int DurationInMinutes { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Service(string name, string? description, int durationInMinutes, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (durationInMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationInMinutes), durationInMinutes, "Duration must be greater than zero.");
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Price cannot be negative.");

        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        DurationInMinutes = durationInMinutes;
        Price = price;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Private parameterless constructor for entity reconstitution.
    private Service()
    {
        Name = null!;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
