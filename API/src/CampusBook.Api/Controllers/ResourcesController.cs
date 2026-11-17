using CampusBook.Api.Data;
using CampusBook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBook.Api.Controllers;

[ApiController]
[Route("api/resources")]
public class ResourcesController : ControllerBase
{
    private readonly ReservationService _reservationService;
    private readonly ResourceService _resourceService;

    public ResourcesController(
        ReservationService reservationService,
        ResourceService resourceService)
    {
        _reservationService = reservationService;
        _resourceService = resourceService;
    }

    [HttpGet]
    public IActionResult GetAvailability([FromQuery] DateTime? date)
    {
        return Ok(_reservationService.GetAvailability(date ?? DateTime.Today));
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var res = _resourceService.GetById(id);

        if (res == null)
            return NotFound();

        return Ok(res);
    }
}
