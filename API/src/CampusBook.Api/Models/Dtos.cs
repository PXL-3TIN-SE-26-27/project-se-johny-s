namespace CampusBook.Api.Models;

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

public class ReservationDto
{
    public int Id { get; set; }
    public string ResourceName { get; set; }
    public string ResourceType { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Status { get; set; }
    public decimal Price { get; set; }
    public decimal Deposit { get; set; }
    public bool Paid { get; set; }
    public string Extras { get; set; }
}

public class ResourceAvailabilityDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Location { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal DayRate { get; set; }
    public bool Available { get; set; }
    public List<string> BusySlots { get; set; } = new List<string>();
}
