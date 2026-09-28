using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reservation.Domain.Entities;

namespace Reservation.Infrastructure.Data.Configurations;

public class ProfessionalServiceConfiguration : IEntityTypeConfiguration<ProfessionalService>
{
    public void Configure(EntityTypeBuilder<ProfessionalService> builder)
    {
        builder.ToTable("ProfessionalServices");

        // The pair is the primary key, which also enforces uniqueness
        // (docs/domain-model.md §6). The DB-level unique constraint is configured here,
        // not in Domain.
        builder.HasKey(ps => new { ps.ProfessionalId, ps.ServiceId });

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
