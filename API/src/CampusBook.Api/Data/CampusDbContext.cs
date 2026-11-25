using CampusBook.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusBook.Api.Data;

public class CampusDbContext(DbContextOptions<CampusDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>()
            .HasDiscriminator<string>("Kind")
            .HasValue<Resource>("active")
            .HasValue<ArchivedResource>("archived");

        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId);
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Resource).WithMany().HasForeignKey(r => r.ResourceId);

        SeedData.Apply(modelBuilder);
    }
}
