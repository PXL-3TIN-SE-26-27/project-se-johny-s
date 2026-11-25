using CampusBook.Api.Data;
using CampusBook.Api.Models;

namespace CampusBook.Api.Services
{
    public class UserService(CampusDbContext db)
    {
        public List<User> GetUsers()
        {
            return db.Users
                .OrderBy(u => u.Name)
                .ToList();
        }
    }
}
