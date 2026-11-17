namespace CampusBook.Api.Models.DTOs;

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
