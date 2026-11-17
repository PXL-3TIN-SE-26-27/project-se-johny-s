namespace CampusBook.Api.Models.DTOs;

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
