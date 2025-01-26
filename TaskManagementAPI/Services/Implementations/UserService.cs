using Microsoft.AspNetCore.Identity;
using TaskManagementAPI.Data;
using TaskManagementAPI.DTO;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _dbContext;

        public UserService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public User ValidateUser(string username, string password)
        {
            return _dbContext.Users
                .FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public void RegisterUser(UserCreateDTO userCreateDTO, int userId)
        {
            var currentUser = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

            if (currentUser == null || !currentUser.IsSuperUser)
            {
                throw new InvalidOperationException("Somente um 'superuser' pode cadastrar novos usuários");
            }

            var newUser = new User
            {
                Username = userCreateDTO.Username,
                Password = userCreateDTO.Password,
                IsSuperUser = userCreateDTO.IsSuperUser
            };

            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();
        }

        public void CreateSuperUser()
        {
            var passwordHasher = new PasswordHasher<User>();

            var superUser = new User
            {
                Username = "su",
                Password = "password",
                IsSuperUser = true
            };

            _dbContext.Users.Add(superUser);
            _dbContext.SaveChanges();
        }
    }
}
