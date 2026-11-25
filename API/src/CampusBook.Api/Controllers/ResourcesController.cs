using CampusBook.Api.Data;
using CampusBook.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampusBook.Api.Controllers;

[ApiController]
[Route("api/resources")]
public class ResourcesController(ReservationService reservationService, ResourceService resourceService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetAvailability([FromQuery] DateTime? date)
    {
        return Ok(reservationService.GetAvailability(date ?? DateTime.Today));
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var res = resourceService.GetById(id);

        if (res == null)
            return NotFound();

        return Ok(res);
    }
}
