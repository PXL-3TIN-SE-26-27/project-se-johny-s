using CampusBook.Api.Data;
using CampusBook.Api.Infrastructure;
using CampusBook.Api.Models;
using CampusBook.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CampusBook.Tests;

public class ReservationTests
{
    [Fact]
    public void Reservation_CanBeCreated()
    {
        var r = new Reservation();
        Assert.NotNull(r);
    }

    [Fact]
    public void Mailer_Sends()
    {
        var mailer = new FakeMailer();
        mailer.Send("lotte.peeters@student.campusbook.local", "Test", "Hello");
        Assert.True(mailer.Sent);
    }

    [Fact]
    public void Cancel_PendingReservation_SetsStatus()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<CampusDbContext>().UseSqlite(connection).Options;
        using var db = new CampusDbContext(options);
        db.Database.EnsureCreated();

        var reservation = new Reservation
        {
            UserId = 1,
            ResourceId = 3,
            Start = new DateTime(2030, 1, 8, 10, 0, 0),
            End = new DateTime(2030, 1, 8, 12, 0, 0),
            Status = "Pending",
            Price = 8m,
            CreatedAt = DateTime.Now
        };
        db.Reservations.Add(reservation);
        db.SaveChanges();

        var service = new ReservationService(new ReservationRepository(db), new PriceCalculator());
        service.Cancel(reservation.Id);

        Assert.Equal("Cancelled", db.Reservations.Find(reservation.Id).Status);
    }

    private class FakeMailer : SmtpMailer
    {
        public bool Sent;

        public FakeMailer() : base("localhost")
        {
        }

        public override void Send(string to, string subject, string body)
        {
            Sent = true;
        }
    }
}
