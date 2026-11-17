namespace CampusBook.Api.Models.DTOs;

public class CreateReservationRequest
{
    public int UserId { get; set; }
    public int ResourceId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public bool Insurance { get; set; }
    public bool Delivery { get; set; }
    public bool Technician { get; set; }
    public bool Cleaning { get; set; }
    public bool ExtendedReturn { get; set; }
    public string Notes { get; set; }
}
