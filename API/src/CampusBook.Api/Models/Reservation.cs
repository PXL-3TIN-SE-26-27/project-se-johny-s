namespace CampusBook.Api.Models;

public class Reservation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int ResourceId { get; set; }
    public Resource Resource { get; set; }

    public DateTime Start { get; set; }
    public DateTime End { get; set; }

    // status
    public string Status { get; set; } = "Pending";

    public bool Insurance { get; set; }
    public bool Delivery { get; set; }
    public bool Technician { get; set; }
    public bool Cleaning { get; set; }
    public bool ExtendedReturn { get; set; }

    public decimal Price { get; set; }
    public decimal Deposit { get; set; }
    public bool Paid { get; set; }
    public string PaymentRef { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public string Notes { get; set; }
}
