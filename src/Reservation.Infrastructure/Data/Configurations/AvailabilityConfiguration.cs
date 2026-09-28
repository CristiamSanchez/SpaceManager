using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservation.Domain.Entities;

namespace Reservation.Infrastructure.Data.Configurations;

public class AvailabilityConfiguration : IEntityTypeConfiguration<Availability>
{
    public void Configure(EntityTypeBuilder<Availability> builder)
    {
        builder.ToTable("Availabilities");

        builder.Property(a => a.DayOfWeek)
            .HasConversion<string>()
            .HasMaxLength(20);

        // TimeOnly maps to PostgreSQL "time" by default.
        builder.Property(a => a.StartTime).IsRequired();
        builder.Property(a => a.EndTime).IsRequired();

        builder.Property(a => a.IsActive).IsRequired();

        // Frequently queried by repository: availability per professional.
        builder.HasIndex(a => a.ProfessionalId);

        builder.HasOne(typeof(Professional), null)
            .WithMany()
            .HasForeignKey("ProfessionalId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
