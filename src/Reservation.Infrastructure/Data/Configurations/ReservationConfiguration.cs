using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservation.Domain.Entities;
using Reservation.Domain.Enums;
using ReservationEntity = Reservation.Domain.Entities.Reservation;

namespace Reservation.Infrastructure.Data.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<ReservationEntity>
{
    public void Configure(EntityTypeBuilder<ReservationEntity> builder)
    {
        builder.ToTable("Reservations");

        // StartAt / EndAt: timestamptz (UTC) — Npgsql default DateTime mapping.
        builder.Property(r => r.StartAt).IsRequired();
        builder.Property(r => r.EndAt).IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.Notes)
            .HasMaxLength(1000);

        // Indexes for frequently queried foreign keys (GetByUserId, GetConflicting).
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.ProfessionalId);
        builder.HasIndex(r => r.ServiceId);

        // Restrict everywhere: deleting a user, professional or service must never
        // cascade-delete historical reservations (docs/domain-model.md §8).
        builder.HasOne(typeof(User), null)
            .WithMany()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(typeof(Professional), null)
            .WithMany()
            .HasForeignKey("ProfessionalId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(typeof(Service), null)
            .WithMany()
            .HasForeignKey("ServiceId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
