namespace CampusBook.Api.Models;

public class Resource
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ResourceTypes Type { get; set; }
    public string Location { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal DayRate { get; set; }

    public virtual void Reserve(Reservation reservation)
    {
        if (reservation.End <= reservation.Start)
            throw new ArgumentException("End before start");
        reservation.ResourceId = Id;
    }

    public virtual bool CanBeDelivered()
    {
        return Type != ResourceTypes.Room;
    }
}
