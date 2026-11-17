using CampusBook.Api.Data;
using CampusBook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBook.Api.Controllers;

[ApiController]
[Route("api/resources")]
public class ResourcesController : ControllerBase
{
    private readonly CampusDbContext _db;

    public ResourcesController(CampusDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult GetAvailability([FromQuery] DateTime? date)
    {
        var service = new ReservationService(new ReservationRepository(_db), _db);
        return Ok(service.GetAvailability(date ?? DateTime.Today));
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var res = _db.Resources.Find(id);
        if (res == null) return NotFound();
        return Ok(res);
    }
}
