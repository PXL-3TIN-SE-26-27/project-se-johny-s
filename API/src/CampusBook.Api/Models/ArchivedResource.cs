namespace CampusBook.Api.Models;

public class ArchivedResource : Resource
{
    public DateTime ArchivedOn { get; set; }
    public string Reason { get; set; }

    public override void Reserve(Reservation reservation)
    {
        throw new NotSupportedException("Resource " + Name + " is archived");
    }

    public override bool CanBeDelivered()
    {
        return false;
    }
}
