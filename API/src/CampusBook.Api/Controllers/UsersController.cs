using CampusBook.Api.Data;
using CampusBook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBook.Api.Controllers;

[ApiController]
[Route("api")]
public class UsersController : ControllerBase
{
    private readonly CampusDbContext _db;

    public UsersController(CampusDbContext db)
    {
        _db = db;
    }

    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        return Ok(_db.Users.OrderBy(u => u.Name).ToList());
    }

    [HttpGet("stats")]
    public IActionResult Stats()
    {
        var stats = new StatsService(new ReservationRepository(_db));
        return Ok(stats.GetDashboard());
    }
}
