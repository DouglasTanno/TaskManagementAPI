using TaskManagementAPI.DTO;
using TaskManagementAPI.Models;

namespace TaskManagementAPI.Services
{
    public interface IUserService
    {
        User ValidateUser(string username, string password);
        void RegisterUser(UserCreateDTO userCreateDTO, int userId);
        void CreateSuperUser();
    }
}
