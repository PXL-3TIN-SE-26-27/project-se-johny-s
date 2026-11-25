using System.Text;
using CampusBook.Api.Models;
using CampusBook.Api.Models.DTOs;
using CampusBook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBook.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController(ReservationService reservationService, ExportService exportService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetForUser([FromQuery] int userId)
    {
        if (userId <= 0) return BadRequest("userId is required");
        return Ok(reservationService.GetForUser(userId));
    }

    [HttpGet("overdue")]
    public IActionResult Overdue()
    {
        return Ok(reservationService.GetOverdue());
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
            var dto = reservationService.Create(request);
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
            var r = reservationService.Confirm(id);
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
            var r = reservationService.CheckOut(id);
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
            var r = reservationService.Return(id);
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
            var r = reservationService.Cancel(id);
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
            var r = reservationService.Pay(id);
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
        var count = reservationService.SendReminders();
        return Ok(new { sent = count });
    }

    [HttpGet("export")]
    public IActionResult Export([FromQuery] int userId)
    {
        if (userId <= 0) return BadRequest("userId is required");

        var csv = exportService.ToCsv(userId);

        return File(
            Encoding.UTF8.GetBytes(csv),
            "text/csv",
            "reservations.csv");
    }
}
