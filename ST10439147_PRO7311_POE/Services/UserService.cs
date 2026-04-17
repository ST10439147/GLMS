using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ST10439147_PRO7311_POE.Data;
using ST10439147_PRO7311_POE.DTOs;
using ST10439147_PRO7311_POE.Hubs;
using ST10439147_PRO7311_POE.Models;

namespace GLMS.Services
{
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;
        private readonly IHubContext<NotificationHub> _hubContext;

        public UserService(
            IDbContextFactory<AppDbContext> dbFactory,
            IHubContext<NotificationHub> hubContext)
        {
            _dbFactory = dbFactory;
            _hubContext = hubContext;
        }

        // ── Login ────────────────────────────────────────────────────────────
        public async Task<User?> Login(LoginDTO loginDto)
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.ApplicationUsers
                .FirstOrDefaultAsync(u =>
                    u.UserName == loginDto.UserName &&
                    u.Password == loginDto.Password);
        }

        // ── Create / Update ──────────────────────────────────────────────────
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

        // ── Read one ─────────────────────────────────────────────────────────
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

        // ── Read all ─────────────────────────────────────────────────────────
        public async Task<List<User>> GetAllUsers()
        {
            using var context = _dbFactory.CreateDbContext();
            return await context.ApplicationUsers.OrderBy(u => u.UserName).ToListAsync();
        }

        // ── Delete ───────────────────────────────────────────────────────────
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

        // ── Helpers ──────────────────────────────────────────────────────────
        private Task Notify(string message) =>
            _hubContext.Clients.All.SendAsync("SignalRmessenger", message);
    }
}
