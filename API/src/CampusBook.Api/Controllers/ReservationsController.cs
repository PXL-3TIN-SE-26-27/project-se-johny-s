using System.Text;
using CampusBook.Api.Data;
using CampusBook.Api.Infrastructure;
using CampusBook.Api.Models;
using CampusBook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBook.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController(ReservationService service, ExportService export, CampusDbContext db) : ControllerBase
{
    [HttpGet]
    public IActionResult GetForUser([FromQuery] int userId)
    {
        if (userId <= 0) return BadRequest("userId is required");
        return Ok(service.GetForUser(userId));
    }

    [HttpGet("overdue")]
    public IActionResult Overdue()
    {
        return Ok(service.GetOverdue());
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateReservationRequest request)
    {
        if (request == null) return BadRequest("No data");
        if (request.UserId <= 0) return BadRequest("UserId is required");
        if (request.ResourceId <= 0) return BadRequest("ResourceId is required");
        if (request.End <= request.Start) return BadRequest("End must be after start");

        try
        {
            var dto = service.Create(request);
            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{id}/confirm")]
    public IActionResult Confirm(int id)
    {
        try
        {
            var r = service.Confirm(id);

            var user = db.Users.Find(r.UserId);
            var resource = db.Resources.Find(r.ResourceId);

            var mailer = new SmtpMailer("smtp.campus.local");
            mailer.Send(user.Email, "CampusBook: reservation confirmed",
                "Hi " + user.Name + ",\n\nYour reservation #" + r.Id + " for " + resource.Name + " on " +
                r.Start.ToString("dd/MM/yyyy HH:mm") + " is confirmed.\nLocation: " + resource.Location + "\n\nCampusBook");

            if (resource.Type == "room" && user.Role == "association")
            {
                mailer.Send(AppConfig.Instance.AdminEmail, "Room booked by association",
                    user.Name + " booked " + resource.Name + " on " + r.Start.ToString("dd/MM/yyyy") +
                    ". Please check opening hours and security.");
            }

            new FileAuditLogger().Log("CONFIRM", r.Id, user.Id);
            StatsService.RegisterConfirmation(resource.Type, r.Price);

            return Ok(new { r.Id, r.Status });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{id}/checkout")]
    public IActionResult CheckOut(int id)
    {
        try
        {
            var r = service.CheckOut(id);
            return Ok(new { r.Id, r.Status });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{id}/return")]
    public IActionResult Return(int id)
    {
        try
        {
            var r = service.Return(id);
            return Ok(new { r.Id, r.Status, r.Price });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{id}/cancel")]
    public IActionResult Cancel(int id)
    {
        try
        {
            var r = service.Cancel(id);
            return Ok(new { r.Id, r.Status });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{id}/pay")]
    public IActionResult Pay(int id)
    {
        try
        {
            var r = service.Pay(id);
            return Ok(new { r.Id, r.Status, r.Paid, r.PaymentRef });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("reminders")]
    public IActionResult Reminders()
    {
        var count = service.SendReminders();
        return Ok(new { sent = count });
    }

    [HttpGet("export")]
    public IActionResult Export([FromQuery] int userId)
    {
        if (userId <= 0) return BadRequest("userId is required");
        var csv = export.ToCsv(userId);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", "reservations.csv");
    }
}
