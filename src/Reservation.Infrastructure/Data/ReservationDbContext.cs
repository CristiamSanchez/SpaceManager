using Microsoft.EntityFrameworkCore;
using Reservation.Domain.Entities;
using ReservationEntity = Reservation.Domain.Entities.Reservation;

namespace Reservation.Infrastructure.Data;

/// <summary>
/// Entry point for Entity Framework Core against PostgreSQL.
/// Maps the Domain model defined in docs/domain-model.md.
/// </summary>
public class ReservationDbContext : DbContext
{
    public ReservationDbContext(DbContextOptions<ReservationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Professional> Professionals => Set<Professional>();
    public DbSet<ProfessionalService> ProfessionalServices => Set<ProfessionalService>();
    public DbSet<Availability> Availabilities => Set<Availability>();
    public DbSet<ReservationEntity> Reservations => Set<ReservationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReservationDbContext).Assembly);
    }
}
