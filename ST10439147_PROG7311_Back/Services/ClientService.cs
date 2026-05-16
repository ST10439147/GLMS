using Microsoft.EntityFrameworkCore;
using ST10439147_PRO7311_POE_Back.Data;
using ST10439147_PRO7311_POE_Back.Models;

namespace ST10439147_PRO7311_POE_Back.Services
{
    // Service layer for managing Clients. This encapsulates all business logic related to clients.
    // This includes validation rules and complex operations like cascading deletes.
    // The controller should call this service for all client-related operations, and the service will handle the underlying logic.
    public class ClientService : IClientService
    {
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public ClientService(IDbContextFactory<AppDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        // Retrieves all clients from the database, ordered by name.
        // The service layer is responsible for any future business rules related to retrieving clients (e.g., filtering, pagination) so that the controller can remain simple.
        public async Task<List<Clients>> GetAllClients()
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Clients.OrderBy(c => c.Name).ToListAsync();
        }

        // Retrieves a single client by ID. The service layer allows us to add any future business rules (e.g., logging, caching) without changing the controller.
        public async Task<Clients?> GetClient(int id)
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.Clients.FindAsync(id);
        }

        // Creates a new client. The service layer is responsible for any validation rules (e.g., required fields, unique constraints) and for returning a consistent response object that the controller can use to generate the HTTP response.
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

        // Updates an existing client. The service layer checks if the client exists before updating and returns a consistent response object.
        // The controller can then use this response to generate the appropriate HTTP response.
        public async Task<ClientResponseDto> UpdateClient(int id, Clients updated)
        {
            // Only updating the client's basic details (name, contact, region) for simplicity.
            // Contracts and service requests are not updated here, but could be handled in their own services.
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

        // Deleting a client is complex due to the relationships with contracts and service requests.
        // Rules:
        //   1. A client can only be deleted when ALL contracts are in a terminal status:
        //      Expired or Draft. Active and OnHold contracts block deletion.
        //   2. For every contract that has service requests, ALL service requests must be
        //      Rejected or Completed before that contract (and therefore the client) can be removed.
        // If the client is eligible for deletion, we explicitly delete all related service
        // requests and contracts in leaf-first order to satisfy FK constraints.
        public async Task<ClientResponseDto> DeleteClient(int id)
        {
            using var ctx = _dbFactory.CreateDbContext();

            // Load client → contracts → service requests so we can validate and
            // explicitly remove children (non-nullable FKs prevent EF from orphaning them).
            var client = await ctx.Clients
                .Include(c => c.Contracts)
                    .ThenInclude(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
                return new ClientResponseDto { Message = $"Client {id} not found.", Status = false };

            // Rule 1: Active and OnHold contracts block client deletion entirely.
            // A contract must be Expired, Draft, or Rejected before the client can be removed.
            var activeContracts = client.Contracts
                .Where(c => c.Status == ContractStatus.Active || c.Status == ContractStatus.OnHold)
                .ToList();

            if (activeContracts.Any())
            {
                var details = string.Join(", ", activeContracts.Select(c => $"#{c.Id} '{c.Title}' ({c.Status})"));
                return new ClientResponseDto
                {
                    Message = $"Cannot delete client '{client.Name}'. " +
                              $"The following contract(s) must be Expired, Draft, or Rejected first: {details}.",
                    Status = false
                };
            }

            // Rule 2: For each contract, all service requests must be Rejected or Completed.
            // A contract with Pending or Approved service requests cannot be removed.
            var contractsWithPendingRequests = client.Contracts
                .Where(c => c.ServiceRequests.Any(sr =>
                    sr.Status != ServiceRequestStatus.Rejected &&
                    sr.Status != ServiceRequestStatus.Completed))
                .ToList();

            if (contractsWithPendingRequests.Any())
            {
                var details = string.Join(", ", contractsWithPendingRequests.Select(c =>
                {
                    var blocking = c.ServiceRequests
                        .Where(sr => sr.Status != ServiceRequestStatus.Rejected && sr.Status != ServiceRequestStatus.Completed)
                        .Select(sr => $"SR#{sr.Id} ({sr.Status})");
                    return $"Contract #{c.Id} '{c.Title}' has open service request(s): {string.Join(", ", blocking)}";
                }));

                return new ClientResponseDto
                {
                    Message = $"Cannot delete client '{client.Name}'. " +
                              $"All service requests must be Rejected or Completed first. {details}.",
                    Status = false
                };
            }

            // Explicitly delete in leaf-first order:
            // ServiceRequests → Contracts → Client
            var allServiceRequests = client.Contracts
                .SelectMany(c => c.ServiceRequests)
                .ToList();

            ctx.ServiceRequests.RemoveRange(allServiceRequests);
            ctx.Contracts.RemoveRange(client.Contracts);
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
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------