using CampusBook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBook.Api.Controllers;

[ApiController]
[Route("api")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly StatsService _statsService;

    public UsersController(
        UserService userService,
        StatsService statsService)
    {
        _userService = userService;
        _statsService = statsService;
    }

    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        return Ok(_userService.GetUsers());
    }

    [HttpGet("stats")]
    public IActionResult Stats()
    {
        return Ok(_statsService.GetDashboard());
    }
}
