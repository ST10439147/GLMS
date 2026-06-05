using ST10439147_PRO7311_POE_Back.Models;
using ST10439147_PRO7311_POE_Back.DTOs;

namespace ST10439147_PRO7311_POE_Back.Services
{
    // IUserService defines the contract for user-related operations, including authentication, user management, and retrieval of user records.
    // The service is used by controllers to perform CRUD operations on user data and to authenticate users based on their credentials.
    public interface IUserService
    {
        Task<User?> Login(LoginDTO loginDto);
        Task<UserResponseDTO> CreateUpdate(User user, int id);
        Task<UserResponseDTO?> GetUserRecord(int id);
        Task<List<User>> GetAllUsers();
        Task<UserResponseDTO> PatchUser(int id, PatchUserDto patch);
        Task<UserResponseDTO?> DeleteUserRecord(int id);
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------