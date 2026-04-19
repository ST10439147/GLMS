using Microsoft.EntityFrameworkCore;
using ST10439147_PRO7311_POE.Data;
using ST10439147_PRO7311_POE.Models;

namespace ST10439147_PRO7311_POE.Services
{
    public class ClientService : IClientService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public ClientService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Clients>> GetAllClients()
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Clients.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Clients?> GetClient(int id)
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Clients.FindAsync(id);
        }

        public async Task<ClientResponseDto> CreateClient(Clients client)
        {
            using var ctx = _dbFactory.CreateDbContext();
            client.Id = 0; // always let the DB assign the PK
            ctx.Clients.Add(client);
            await ctx.SaveChangesAsync();
            return new ClientResponseDto
            {
                Client = client,
                Message = $"Client '{client.Name}' created.",
                Status = true
            };
        }

        public async Task<ClientResponseDto> UpdateClient(int id, Clients updated)
        {
            using var ctx = _dbFactory.CreateDbContext();
            var existing = await ctx.Clients.FindAsync(id);
            if (existing == null)
                return new ClientResponseDto { Message = $"Client {id} not found.", Status = false };

            existing.Name = updated.Name;
            existing.ContactDetails = updated.ContactDetails;
            existing.Region = updated.Region;
            await ctx.SaveChangesAsync();

            return new ClientResponseDto
            {
                Client = existing,
                Message = $"Client '{existing.Name}' updated.",
                Status = true
            };
        }

        public async Task<ClientResponseDto> DeleteClient(int id)
        {
            using var ctx = _dbFactory.CreateDbContext();
            var client = await ctx.Clients.FindAsync(id);
            if (client == null)
                return new ClientResponseDto { Message = $"Client {id} not found.", Status = false };

            ctx.Clients.Remove(client);
            await ctx.SaveChangesAsync();

            return new ClientResponseDto
            {
                Client = client,
                Message = $"Client '{client.Name}' deleted.",
                Status = true
            };
        }
    }
}