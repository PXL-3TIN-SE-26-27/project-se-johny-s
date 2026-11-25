using CampusBook.Api.Models;

namespace CampusBook.Api.Data;

public interface IReservationRepository
{
    Reservation GetById(int id);
    List<Reservation> GetByUser(int userId);
    List<Reservation> GetByResource(int resourceId);
    List<Reservation> GetByDateRange(DateTime from, DateTime to);
    List<Reservation> GetByStatus(ReservationStatus status);
    List<Reservation> GetActiveForResource(int resourceId);
    List<Reservation> GetOverdue(DateTime now);
    List<Reservation> GetUnpaid();
    bool HasOverlap(int resourceId, DateTime start, DateTime end);
    Dictionary<ReservationStatus, int> CountByStatus();
    Dictionary<ResourceTypes, int> CountByResourceType();
    void Add(Reservation reservation);
    void Update(Reservation reservation);
    void Delete(int id);
    void SaveChanges();
    User GetUser(int id);
    Resource GetResource(int id);
    List<Resource> GetAllResources();
}
