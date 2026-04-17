using ST10439147_PRO7311_POE.Models;
using ST10439147_PRO7311_POE.DTOs;

namespace GLMS.Services
{
    public interface IUserService
    {
        Task<User?> Login(LoginDTO loginDto);
        Task<UserResponseDTO> CreateUpdate(User user, int id);
        Task<UserResponseDTO?> GetUserRecord(int id);
        Task<List<User>> GetAllUsers();
        Task<UserResponseDTO?> DeleteUserRecord(int id);
    }
}
