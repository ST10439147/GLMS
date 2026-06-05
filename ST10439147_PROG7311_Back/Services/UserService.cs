using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ST10439147_PRO7311_POE_Back.Data;
using ST10439147_PRO7311_POE_Back.DTOs;
using ST10439147_PRO7311_POE_Back.Hubs;
using ST10439147_PRO7311_POE_Back.Models;

namespace ST10439147_PRO7311_POE_Back.Services
{
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly IHubContext<NotificationHub> _hubContext;

        public UserService(
            // Injecting the database context factory which allows for efficient creation of DbContext instances as needed, without holding onto a long-lived context.
            IDbContextFactory<AppDbContext> dbFactory,
            IHubContext<NotificationHub> hubContext)
        {
            _dbFactory = dbFactory;
            _hubContext = hubContext;
        }

        // Attempts to authenticate a user with the specified login credentials.
        // The method returns <see langword="null"/> if no user matches the provided credentials.
        // The password is compared directly; ensure credentials are handled securely.
        // An object containing the user's login credentials with LoginDTO. The user name and password must be provided.
        // A object representing the authenticated user if the credentials are valid; otherwise, null.
        // If the user is not found or the credentials are invalid, the method returns.
        public async Task<User?> Login(LoginDTO loginDto)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.ApplicationUsers
                .FirstOrDefaultAsync(u =>
                    u.UserName == loginDto.UserName &&
                    u.Password == loginDto.Password);
        }

        // Creates a new user or updates an existing user with the specified identifier.
        // If a user with the specified identifier does not exist, a new user is created.
        // Otherwise, the existing user's details are updated. The response message indicates the result of the operation.
        // The user entity containing the details to create or update. Cannot be null.
        // The identifier of the user to update. If no user with this identifier exists, a new user is created.
        // A UserResponseDTO containing the user data, operation status, and a message indicating whether the user was created or updated.
        public async Task<UserResponseDTO> CreateUpdate(User user, int id)
        {
            using var context = _dbFactory.CreateDbContext();

            var existing = await context.ApplicationUsers.FindAsync(id);
            var response = new UserResponseDTO { User = user, Status = true };

            if (existing == null)
            {
                user.Id = 0; // let SQL Server assign the PK
                context.ApplicationUsers.Add(user);
                await context.SaveChangesAsync();
                response.Message = $"User created: {user.UserName} | Role: {user.Role}";
            }
            else
            {
                context.Entry(existing).CurrentValues.SetValues(user);
                await context.SaveChangesAsync();
                response.Message = $"User updated: {user.UserName} | Role: {user.Role}";
            }

            await Notify(response.Message);
            return response;
        }

        // Retrieves the user record with the specified identifier from the database.
        // If a user with the specified identifier is found, a response containing the user data and a success message is returned; otherwise, a response indicating that the user was not found is returned.
        public async Task<UserResponseDTO?> GetUserRecord(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            var user = await context.ApplicationUsers.FindAsync(id);

            var response = new UserResponseDTO { User = user };
            if (user != null)
            {
                response.Status = true;
                response.Message = $"User found: {user.UserName} | Role: {user.Role}";
            }
            else
            {
                response.Status = false;
                response.Message = $"User not found – Id: {id}";
            }

            await Notify(response.Message);
            return response;
        }

        // Applies only the non-null fields from the patch DTO, leaving everything else unchanged.
        public async Task<UserResponseDTO> PatchUser(int id, PatchUserDto patch)
        {
            using var context = _dbFactory.CreateDbContext();
            var existing = await context.ApplicationUsers.FindAsync(id);
            var response = new UserResponseDTO();

            if (existing == null)
            {
                response.Status = false;
                response.Message = $"User not found – Id: {id}";
                return response;
            }

            if (patch.FirstName is not null) existing.FirstName = patch.FirstName;
            if (patch.LastName is not null) existing.LastName = patch.LastName;
            if (patch.UserName is not null) existing.UserName = patch.UserName;
            if (patch.Email is not null) existing.Email = patch.Email;
            if (patch.Password is not null) existing.Password = patch.Password;
            if (patch.AccessLevel.HasValue) existing.AccessLevel = patch.AccessLevel.Value;

            await context.SaveChangesAsync();

            response.User = existing;
            response.Status = true;
            response.Message = $"User patched: {existing.UserName} | Role: {existing.Role}";

            await Notify(response.Message);
            return response;
        }

        // Retrieves a list of all users from the database, ordered by their user names.
        public async Task<List<User>> GetAllUsers()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.ApplicationUsers.OrderBy(u => u.UserName).ToListAsync();
        }

        // Deletes the user record with the specified identifier from the database.
        // If the user is found and deleted successfully, a response indicating success is returned; otherwise, a response indicating failure is returned.

        public async Task<UserResponseDTO?> DeleteUserRecord(int id)
        {
            using var context = _dbFactory.CreateDbContext();
            var user = await context.ApplicationUsers.FindAsync(id);
            var response = new UserResponseDTO { User = user };

            if (user != null)
            {
                context.ApplicationUsers.Remove(user);
                await context.SaveChangesAsync();
                response.Status = true;
                response.Message = $"User deleted: {user.UserName}";
            }
            else
            {
                response.Status = false;
                response.Message = $"User not found – Id: {id}";
            }

            await Notify(response.Message);
            return response;
        }

        // Sends a notification message to all connected clients via SignalR.
        // The message is broadcast to all clients subscribed to the "SignalRmessenger" event, allowing real-time updates in the UI.
        private Task Notify(string message) =>
            _hubContext.Clients.All.SendAsync("SignalRmessenger", message);
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------