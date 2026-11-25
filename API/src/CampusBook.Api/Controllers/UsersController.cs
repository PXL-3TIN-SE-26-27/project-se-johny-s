using CampusBook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBook.Api.Controllers;

[ApiController]
[Route("api")]
public class UsersController(UserService userService, StatsService statsService) : ControllerBase
{

    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        return Ok(userService.GetUsers());
    }

    [HttpGet("stats")]
    public IActionResult Stats()
    {
        return Ok(statsService.GetDashboard());
    }
}
