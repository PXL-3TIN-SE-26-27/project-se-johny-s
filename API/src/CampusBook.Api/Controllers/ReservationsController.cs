using System.Text;
using CampusBook.Api.Models;
using CampusBook.Api.Models.DTOs;
using CampusBook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBook.Api.Controllers;

[ApiController]
[Route("api/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationService _service;
    private readonly ExportService _exportService;

    public ReservationsController(ReservationService service, ExportService exportService)
    {
        _service = service;
        _exportService = exportService;
    }

    [HttpGet]
    public IActionResult GetForUser([FromQuery] int userId)
    {
        if (userId <= 0) return BadRequest("userId is required");
        return Ok(_service.GetForUser(userId));
    }

    [HttpGet("overdue")]
    public IActionResult Overdue()
    {
        return Ok(_service.GetOverdue());
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
            var dto = _service.Create(request);
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
            var r = _service.Confirm(id);
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
            var r = _service.CheckOut(id);
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
            var r = _service.Return(id);
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
            var r = _service.Cancel(id);
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
            var r = _service.Pay(id);
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
        var count = _service.SendReminders();
        return Ok(new { sent = count });
    }

    [HttpGet("export")]
    public IActionResult Export([FromQuery] int userId)
    {
        if (userId <= 0) return BadRequest("userId is required");

        var csv = _exportService.ToCsv(userId);

        return File(
            Encoding.UTF8.GetBytes(csv),
            "text/csv",
            "reservations.csv");
    }
}
