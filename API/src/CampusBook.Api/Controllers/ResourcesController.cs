using CampusBook.Api.Data;
using CampusBook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBook.Api.Controllers;

[ApiController]
[Route("api/resources")]
public class ResourcesController(ReservationService service, CampusDbContext db) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAvailability([FromQuery] DateTime? date)
    {
        return Ok(service.GetAvailability(date ?? DateTime.Today));
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var res = db.Resources.Find(id);
        if (res == null) return NotFound();
        return Ok(res);
    }
}
