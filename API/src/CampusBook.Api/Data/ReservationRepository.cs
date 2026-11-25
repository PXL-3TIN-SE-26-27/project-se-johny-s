using CampusBook.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CampusBook.Api.Data;

public class ReservationRepository(CampusDbContext db) : IReservationRepository
{
    public Reservation GetById(int id)
    {
        return db.Reservations.Include(r => r.Resource).Include(r => r.User).FirstOrDefault(r => r.Id == id);
    }

    public List<Reservation> GetByUser(int userId)
    {
        return db.Reservations.Include(r => r.Resource)
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.Start)
            .ToList();
    }

    public List<Reservation> GetByResource(int resourceId)
    {
        return db.Reservations.Where(r => r.ResourceId == resourceId).OrderBy(r => r.Start).ToList();
    }

    public List<Reservation> GetByDateRange(DateTime from, DateTime to)
    {
        return db.Reservations.Where(r => r.Start < to && r.End > from).ToList();
    }

    public List<Reservation> GetByStatus(ReservationStatus status)
    {
        return db.Reservations.Where(r => r.Status == status).ToList();
    }

    public List<Reservation> GetActiveForResource(int resourceId)
    {
        return db.Reservations
            .Where(r => r.ResourceId == resourceId && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed || r.Status == ReservationStatus.CheckedOut))
            .ToList();
    }

    public List<Reservation> GetOverdue(DateTime now)
    {
        return db.Reservations.Include(r => r.Resource).Include(r => r.User)
            .Where(r => r.Status == ReservationStatus.CheckedOut && r.End < now)
            .ToList();
    }

    public List<Reservation> GetUnpaid()
    {
        return db.Reservations.Where(r => !r.Paid && r.Status != ReservationStatus.Cancelled).ToList();
    }

    public bool HasOverlap(int resourceId, DateTime start, DateTime end)
    {
        return db.Reservations.Any(r => r.ResourceId == resourceId
                                         && r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Returned
                                         && r.Start < end && r.End > start);
    }

    public Dictionary<ReservationStatus, int> CountByStatus()
    {
        return db.Reservations.GroupBy(r => r.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionary(x => x.Key, x => x.Count);
    }

    public Dictionary<ResourceTypes, int> CountByResourceType()
    {
        return db.Reservations.Include(r => r.Resource).ToList()
            .GroupBy(r => r.Resource.Type)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public void Add(Reservation reservation)
    {
        db.Reservations.Add(reservation);
    }

    public void Update(Reservation reservation)
    {
        db.Reservations.Update(reservation);
    }

    public void Delete(int id)
    {
        var r = db.Reservations.Find(id);
        if (r != null) db.Reservations.Remove(r);
    }

    public void SaveChanges()
    {
        db.SaveChanges();
    }

    public User GetUser(int id)
    {
        return db.Users.Find(id);
    }

    public Resource GetResource(int id)
    {
        return db.Resources.Find(id);
    }

    public List<Resource> GetAllResources()
    {
        return db.Resources.ToList();
    }
}
