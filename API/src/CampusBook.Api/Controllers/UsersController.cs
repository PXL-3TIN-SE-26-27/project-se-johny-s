using CampusBook.Api.Data;
using CampusBook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBook.Api.Controllers;

[ApiController]
[Route("api")]
public class UsersController(StatsService stats, CampusDbContext db) : ControllerBase
{
    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        return Ok(db.Users.OrderBy(u => u.Name).ToList());
    }

    [HttpGet("stats")]
    public IActionResult Stats()
    {
        return Ok(stats.GetDashboard());
    }
}
