using CampusBook.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusBook.Api.Data;

public class ReservationRepository : IReservationRepository
{
    private readonly CampusDbContext _db;

    public ReservationRepository(CampusDbContext db)
    {
        _db = db;
    }

    public Reservation GetById(int id)
    {
        return _db.Reservations.Include(r => r.Resource).Include(r => r.User).FirstOrDefault(r => r.Id == id);
    }

    public List<Reservation> GetByUser(int userId)
    {
        return _db.Reservations.Include(r => r.Resource)
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.Start)
            .ToList();
    }

    public List<Reservation> GetByResource(int resourceId)
    {
        return _db.Reservations.Where(r => r.ResourceId == resourceId).OrderBy(r => r.Start).ToList();
    }

    public List<Reservation> GetByDateRange(DateTime from, DateTime to)
    {
        return _db.Reservations.Where(r => r.Start < to && r.End > from).ToList();
    }

    public List<Reservation> GetByStatus(string status)
    {
        return _db.Reservations.Where(r => r.Status == status).ToList();
    }

    public List<Reservation> GetActiveForResource(int resourceId)
    {
        return _db.Reservations
            .Where(r => r.ResourceId == resourceId && (r.Status == "Pending" || r.Status == "Confirmed" || r.Status == "CheckedOut"))
            .ToList();
    }

    public List<Reservation> GetOverdue(DateTime now)
    {
        return _db.Reservations.Include(r => r.Resource).Include(r => r.User)
            .Where(r => r.Status == "CheckedOut" && r.End < now)
            .ToList();
    }

    public List<Reservation> GetUnpaid()
    {
        return _db.Reservations.Where(r => !r.Paid && r.Status != "Cancelled").ToList();
    }

    public bool HasOverlap(int resourceId, DateTime start, DateTime end)
    {
        return _db.Reservations.Any(r => r.ResourceId == resourceId
                                         && r.Status != "Cancelled" && r.Status != "Returned"
                                         && r.Start < end && r.End > start);
    }

    public Dictionary<string, int> CountByStatus()
    {
        return _db.Reservations.GroupBy(r => r.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionary(x => x.Key, x => x.Count);
    }

    public Dictionary<string, int> CountByResourceType()
    {
        return _db.Reservations.Include(r => r.Resource).ToList()
            .GroupBy(r => r.Resource.Type)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public void Add(Reservation reservation)
    {
        _db.Reservations.Add(reservation);
    }

    public void Update(Reservation reservation)
    {
        _db.Reservations.Update(reservation);
    }

    public void Delete(int id)
    {
        var r = _db.Reservations.Find(id);
        if (r != null) _db.Reservations.Remove(r);
    }

    public void SaveChanges()
    {
        _db.SaveChanges();
    }

    public User GetUser(int id)
    {
        return _db.Users.Find(id);
    }

    public Resource GetResource(int id)
    {
        return _db.Resources.Find(id);
    }

    public List<Resource> GetAllResources()
    {
        return _db.Resources.ToList();
    }
}
