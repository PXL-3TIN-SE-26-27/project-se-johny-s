using CampusBook.Api.Data;
using CampusBook.Api.Models;

namespace CampusBook.Api.Services;

public class ResourceService(IReservationRepository repository)
{
    public Resource GetById(int id)
    {
        return repository.GetResource(id);
    }
}
