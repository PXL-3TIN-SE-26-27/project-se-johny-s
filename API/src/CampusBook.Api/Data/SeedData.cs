using CampusBook.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusBook.Api.Data;

public static class SeedData
{

    public static void Apply(ModelBuilder mb)
    {
        mb.Entity<User>().HasData(
            new User { Id = 1, Name = "Lotte Peeters", Email = "lotte.peeters@student.campusbook.local", UserRole = UserRole.Student },
            new User { Id = 2, Name = "Jonas De Smet", Email = "jonas.desmet@student.campusbook.local", UserRole = UserRole.Student },
            new User { Id = 3, Name = "Amira El Idrissi", Email = "amira.elidrissi@student.campusbook.local", UserRole = UserRole.Student },
            new User { Id = 4, Name = "Dirk Vermeulen", Email = "dirk.vermeulen@campusbook.local", UserRole = UserRole.Staff },
            new User { Id = 5, Name = "Sofie Claes", Email = "sofie.claes@campusbook.local", UserRole = UserRole.Staff },
            new User { Id = 6, Name = "Studentenkring Informatica", Email = "praesidium@kring.campusbook.local", UserRole = UserRole.Association },
            new User { Id = 7, Name = "Fotoclub Diafragma", Email = "bestuur@diafragma.campusbook.local", UserRole = UserRole.Association }
        );

        mb.Entity<Resource>().HasData(
            new Resource { Id = 1, Name = "Lokaal B.204", Type = ResourceTypes.Room, Location = "Blok B", HourlyRate = 12m, DayRate = 80m },
            new Resource { Id = 2, Name = "Auditorium A", Type = ResourceTypes.Room, Location = "Blok A", HourlyRate = 40m, DayRate = 260m },
            new Resource { Id = 3, Name = "Laptop L-01", Type = ResourceTypes.Laptop, Location = "Uitleenbalie", HourlyRate = 4m, DayRate = 25m },
            new Resource { Id = 4, Name = "Laptop L-02", Type = ResourceTypes.Laptop, Location = "Uitleenbalie", HourlyRate = 4m, DayRate = 25m },
            new Resource { Id = 5, Name = "Laptop L-03", Type = ResourceTypes.Laptop, Location = "Uitleenbalie", HourlyRate = 4m, DayRate = 25m },
            new Resource { Id = 6, Name = "Canon R6 kit", Type = ResourceTypes.Camera, Location = "Mediatheek", HourlyRate = 9m, DayRate = 55m },
            new Resource { Id = 7, Name = "Sony A7 IV", Type = ResourceTypes.Camera, Location = "Mediatheek", HourlyRate = 10m, DayRate = 60m },
            new Resource { Id = 8, Name = "Prusa MK4", Type = ResourceTypes.Printer, Location = "Makerspace", HourlyRate = 6m, DayRate = 40m }
        );

        mb.Entity<ArchivedResource>().HasData(
            new ArchivedResource
            {
                Id = 9,
                Name = "Nikon D3200",
                Type = ResourceTypes.Camera,
                Location = "Mediatheek",
                HourlyRate = 5m,
                DayRate = 30m,
                ArchivedOn = new DateTime(2025, 6, 30),
                Reason = "Sensor defect"
            }
        );

        mb.Entity<Reservation>().HasData(
            new Reservation
            {
                Id = 1,
                UserId = 1,
                ResourceId = 3,
                Start = new DateTime(2026, 10, 5, 9, 0, 0),
                End = new DateTime(2026, 10, 5, 13, 0, 0),
                Status = ReservationStatus.Returned,
                Price = 16m,
                Deposit = 0m,
                Paid = true,
                PaymentRef = "PL0000000001R1",
                CreatedAt = new DateTime(2026, 10, 1, 14, 2, 0),
                CheckedOutAt = new DateTime(2026, 10, 5, 8, 55, 0),
                ReturnedAt = new DateTime(2026, 10, 5, 12, 40, 0)
            },
            new Reservation
            {
                Id = 2,
                UserId = 6,
                ResourceId = 2,
                Start = new DateTime(2026, 11, 18, 18, 0, 0),
                End = new DateTime(2026, 11, 18, 23, 0, 0),
                Status = ReservationStatus.Confirmed,
                Price = 208m,
                Deposit = 0m,
                Paid = false,
                CreatedAt = new DateTime(2026, 10, 2, 10, 15, 0),
                Notes = "Kerstquiz"
            },
            new Reservation
            {
                Id = 3,
                UserId = 2,
                ResourceId = 6,
                Start = new DateTime(2026, 11, 7, 10, 0, 0),
                End = new DateTime(2026, 11, 7, 16, 0, 0),
                Status = ReservationStatus.Pending,
                Price = 67.5m,
                Deposit = 150m,
                Paid = false,
                Insurance = false,
                CreatedAt = new DateTime(2026, 10, 9, 20, 41, 0)
            },
            new Reservation
            {
                Id = 4,
                UserId = 4,
                ResourceId = 8,
                Start = new DateTime(2026, 10, 20, 13, 0, 0),
                End = new DateTime(2026, 10, 20, 17, 0, 0),
                Status = ReservationStatus.Confirmed,
                Price = 0m,
                Deposit = 0m,
                Paid = true,
                PaymentRef = "FREE",
                CreatedAt = new DateTime(2026, 10, 8, 9, 0, 0)
            }
        );
    }

}
