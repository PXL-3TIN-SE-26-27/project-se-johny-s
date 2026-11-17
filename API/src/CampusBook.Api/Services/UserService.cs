using CampusBook.Api.Data;
using CampusBook.Api.Models;

namespace CampusBook.Api.Services
{
    public class UserService
    {
        private readonly CampusDbContext _db;

        public UserService(CampusDbContext db)
        {
            _db = db;
        }

        public List<User> GetUsers()
        {
            return _db.Users
                .OrderBy(u => u.Name)
                .ToList();
        }
    }
}
