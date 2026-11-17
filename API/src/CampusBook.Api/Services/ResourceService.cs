using CampusBook.Api.Data;
using CampusBook.Api.Models;

namespace CampusBook.Api.Services
{
    public class ResourceService
    {
        private readonly IReservationRepository _repository;

        public ResourceService(IReservationRepository repository)
        {
            _repository = repository;
        }

        public Resource GetById(int id)
        {
            return _repository.GetResource(id);
        }
    }
}
