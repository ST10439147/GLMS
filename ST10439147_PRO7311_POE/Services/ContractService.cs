using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ST10439147_PRO7311_POE.Data;
using ST10439147_PRO7311_POE.DTOs;
using ST10439147_PRO7311_POE.Hubs;
using ST10439147_PRO7311_POE.Models;
namespace ST10439147_PRO7311_POE.Services
{
    // The ContractService class implements the IContractService interface and provides methods for managing contracts and their associated service requests.
    // It uses Entity Framework Core for database operations and SignalR for real-time notifications.
    // The service includes methods for creating, retrieving, updating, and deleting contracts, as well as managing service requests under those contracts.
    // It also includes functionality to search for contracts based on specific criteria and to save signed agreements.
    public class ContractService : IContractService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IDbContextFactory<AppDbContext> _dbFactory;

        public ContractService(
            IHubContext<NotificationHub> hubContext,
            IDbContextFactory<AppDbContext> dbFactory)
        {
            _hubContext = hubContext;
            _dbFactory = dbFactory;
        }

        // Creates a new contract with the provided details and saves it to the database. The method sets the initial status of the contract to Draft, adds it to the database context, and saves the changes.
        // After successfully creating the contract, it sends a notification using SignalR to inform clients about the new contract creation.
        // The method returns a response object containing the created contract, a success status, and a message confirming the creation.
        public async Task<ContractResponseDto> CreateContract(Contract contract)
        {
            using var ctx = _dbFactory.CreateDbContext();
            contract.Status = ContractStatus.Draft;
            ctx.Contracts.Add(contract);
            await ctx.SaveChangesAsync();

            var msg = $"Contract created: '{contract.Title}' [Draft]";
            await Notify(msg, contract.Id, contract.Status);
            return new ContractResponseDto { Contract = contract, Message = msg, Status = true };
        }

        // Retrieves a contract by its ID, including related client and service request information.
        // The method queries the database for a contract matching the provided ID and includes related entities such as the client and service requests.
        // If the contract is found, it returns a response object containing the contract details, a success status, and a message indicating that the contract was found.
        // If the contract is not found, it returns a response object with a failure status and a message indicating that the contract was not found.
        public async Task<ContractResponseDto?> GetContract(int id)
        {
            using var ctx = _dbFactory.CreateDbContext();
            var contract = await ctx.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return new ContractResponseDto { Message = $"Contract {id} not found.", Status = false };

            // Auto-correct status on read
            await ApplyAndSave(contract, ctx);

            return new ContractResponseDto { Contract = contract, Message = "Found.", Status = true };
        }

        // Retrieves all contracts from the database, including related client information.
        // The method returns a list of contracts, each with its associated client details.
        // If no contracts are found, it returns an empty list.
        // The method uses Entity Framework Core to query the database and includes the related client information for each contract.
        // The resulting list of contracts is returned to the caller.
        public async Task<List<Contract>> GetAllContracts()
        {
            using var ctx = _dbFactory.CreateDbContext();
            var contracts = await ctx.Contracts.Include(c => c.Client).ToListAsync();

            foreach (var c in contracts)
                await ApplyAndSave(c, ctx);

            return contracts;
        }

        // Updates an existing contract with new details provided in the updated contract object.
        // The method first retrieves the existing contract from the database using the provided ID.
        // If the contract is found, it updates the contract's properties with the new values and saves the changes to the database.
        // The method then sends a notification using SignalR to inform clients about the contract update.
        // The method returns a response object containing the updated contract, a success status, and a message confirming the update.
        // If the contract is not found, it returns a response object with a failure status and a message indicating that the contract was not found.
        public async Task<ContractResponseDto> UpdateContract(int id, Contract updated)
        {
            using var ctx = _dbFactory.CreateDbContext();
            var existing = await ctx.Contracts.FindAsync(id);
            if (existing == null)
                return new ContractResponseDto { Message = $"Contract {id} not found.", Status = false };

            existing.Title = updated.Title;
            existing.ServiceLevel = updated.ServiceLevel;
            existing.ClientId = updated.ClientId;
            existing.StartDate = updated.StartDate;
            existing.EndDate = updated.EndDate;
            await ctx.SaveChangesAsync();

            var msg = $"Contract {id} updated.";
            await Notify(msg, id, existing.Status);
            return new ContractResponseDto { Contract = existing, Message = msg, Status = true };
        }

        // Updates the status of a specific contract identified by its ID.
        // The method takes the contract ID and the new status as parameters, and it updates the status of the corresponding contract in the database.
        // If the contract is found and updated successfully, the method returns a response object containing the updated contract, a success status, and a message indicating the change.
        // If the contract is not found, it returns a failure response with an appropriate message.
        // The method also sends a notification using SignalR to inform clients about the status change of the contract.
        public async Task<ContractResponseDto> SetContractStatus(int contractId, ContractStatus newStatus)
        {
            using var ctx = _dbFactory.CreateDbContext();
            var contract = await ctx.Contracts.FindAsync(contractId);
            if (contract == null)
                return new ContractResponseDto { Message = $"Contract {contractId} not found.", Status = false };

            contract.Status = newStatus;
            await ctx.SaveChangesAsync();

            var msg = $"Contract '{contract.Title}' status → {newStatus}";
            await Notify(msg, contractId, newStatus);
            return new ContractResponseDto { Contract = contract, Message = msg, Status = true };
        }

        // Deletes a specific contract identified by its ID, subject to two rules:
        //   Rule 1 - The contract's status must be Expired, Draft, or Rejected.
        //            Active and OnHold contracts cannot be deleted.
        //   Rule 2 - All associated service requests must be Rejected or Completed
        //            (or there are none) before the contract can be removed.
        // The method returns a descriptive failure message when either rule is violated.
        public async Task<ContractResponseDto?> DeleteContract(int id)
        {
            using var ctx = _dbFactory.CreateDbContext();

            // Load contract together with its service requests so we can validate
            var contract = await ctx.Contracts
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
                return new ContractResponseDto { Message = $"Contract {id} not found.", Status = false };

            // Rule 1: Only terminal-status contracts may be deleted.
            // Active and OnHold contracts represent ongoing obligations and must be
            // transitioned to Expired, Draft, or Rejected before deletion is allowed.
            if (contract.Status == ContractStatus.Active || contract.Status == ContractStatus.OnHold)
            {
                return new ContractResponseDto
                {
                    Message = $"Cannot delete contract '{contract.Title}'. " +
                              $"Its current status is '{contract.Status}'. " +
                              "A contract must be Expired, Draft, or Rejected before it can be deleted.",
                    Status = false
                };
            }

            // Rule 2: All service requests must be closed before the contract can be removed.
            var blocking = contract.ServiceRequests
                .Where(sr => sr.Status != ServiceRequestStatus.Rejected &&
                             sr.Status != ServiceRequestStatus.Completed)
                .ToList();

            if (blocking.Any())
            {
                var ids = string.Join(", ", blocking.Select(sr => $"#{sr.Id} ({sr.Status})"));
                return new ContractResponseDto
                {
                    Message = $"Cannot delete contract '{contract.Title}'. " +
                              $"The following service request(s) must be Rejected or Completed first: {ids}.",
                    Status = false
                };
            }

            // Explicitly remove child service requests before removing the contract.
            // This is required because the FK on ServiceRequest.ContractId is non-nullable,
            // so EF Core cannot orphan them — it must delete them or throw.
            ctx.ServiceRequests.RemoveRange(contract.ServiceRequests);
            ctx.Contracts.Remove(contract);
            await ctx.SaveChangesAsync();

            var msg = $"Contract {id} '{contract.Title}' deleted.";
            await Notify(msg, id, contract.Status);
            return new ContractResponseDto { Contract = contract, Message = msg, Status = true };
        }

        // Derives the correct runtime status for a contract based on business rules:
        // 1. If EndDate has passed → Expired (takes priority)
        // 2. If no signed agreement is uploaded → Draft
        // The persisted Status is NOT overwritten here; this is a read-time projection.
        // Call ApplyAndSave to persist the corrected status when needed.
        private static ContractStatus ResolveStatus(Contract contract)
        {
            if (contract.EndDate < DateTime.UtcNow)
                return ContractStatus.Expired;

            if (string.IsNullOrEmpty(contract.SignedAgreementPath))
                return ContractStatus.Draft;

            return contract.Status;
        }

        // Checks whether the resolved status differs from the stored one,
        // and if so saves the correction to the database. Returns the contract.
        private async Task<Contract> ApplyAndSave(Contract contract, AppDbContext ctx)
        {
            var resolved = ResolveStatus(contract);
            if (contract.Status != resolved)
            {
                contract.Status = resolved;
                await ctx.SaveChangesAsync();
                await Notify($"Contract '{contract.Title}' auto-status → {resolved}",
                             contract.Id, resolved);
            }
            return contract;
        }
        // Search for contracts based on optional criteria: start date range and status.
        // The method accepts three parameters: startFrom, startTo, and status.
        // It constructs a query to filter contracts based on the provided criteria.
        // If startFrom is specified, it filters for contracts with a StartDate greater than or equal to startFrom.
        // If startTo is specified, it filters for contracts with a StartDate less than or equal to startTo.
        // If status is specified, it filters for contracts that match the given status.
        // The resulting list of contracts is ordered by StartDate and returned as a response.
        public async Task<List<Contract>> SearchContracts(
    DateTime? startFrom, DateTime? startTo, ContractStatus? status)
        {
            using var ctx = _dbFactory.CreateDbContext();
            IQueryable<Contract> query = ctx.Contracts.Include(c => c.Client);

            if (startFrom.HasValue)
                query = query.Where(c => c.StartDate >= startFrom.Value);
            if (startTo.HasValue)
                query = query.Where(c => c.StartDate <= startTo.Value);
            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);

            var results = await query.OrderBy(c => c.StartDate).ToListAsync();

            // expired/draft contracts may still appear if their stored status
            // matched the search filter. This is intentional — it corrects the DB.
            foreach (var c in results)
                await ApplyAndSave(c, ctx);

            return results;
        }

        // Saves the file path and original file name of a signed agreement for a specific contract.
        // The method takes the contract ID, the file path where the signed agreement is stored, and the original file name as parameters.
        // It updates the corresponding contract record in the database with this information.
        // If the contract is found and updated successfully, it returns a response object containing the updated contract, a success status, and a message confirming that the signed agreement has been saved.
        // If the contract is not found, it returns a failure response with an appropriate message.
        public async Task<ContractResponseDto> SaveSignedAgreement(
            int contractId, string filePath, string originalFileName)
        {
            using var ctx = _dbFactory.CreateDbContext();
            var contract = await ctx.Contracts.FindAsync(contractId);
            if (contract == null)
                return new ContractResponseDto { Message = $"Contract {contractId} not found.", Status = false };

            contract.SignedAgreementPath = filePath;
            contract.SignedAgreementFileName = originalFileName;
            await ctx.SaveChangesAsync();

            return new ContractResponseDto
            {
                Contract = contract,
                Message = $"Signed agreement saved for contract '{contract.Title}'.",
                Status = true
            };
        }


        // Creates a new service request associated with the specified contract.
        // A service request can only be created for contracts that are in the Active status. 
        // If the contract is expired or on hold, the operation does not create a service request and returns a failure response.
        // The method also sends a notification upon successful creation.
        // The service request to create. The associated contract must exist and be in the Active status.</param>
        // A response object containing the created service request, a status indicator, and a message describing the result.
        // If the contract does not exist or is not active, the response indicates failure and includes an explanatory message.
        public async Task<ServiceRequestResponseDto> CreateServiceRequest(ServiceRequest request)
        {
            using var ctx = _dbFactory.CreateDbContext();

            var contract = await ctx.Contracts.FindAsync(request.ContractId);
            if (contract == null)
                return new ServiceRequestResponseDto
                {
                    Message = $"Contract {request.ContractId} not found.",
                    Status = false
                };

            if (contract.Status == ContractStatus.Expired || contract.Status == ContractStatus.OnHold)
                return new ServiceRequestResponseDto
                {
                    Message = $"Cannot create service request: contract is '{contract.Status}'. " +
                              "Only Active contracts accept service requests.",
                    Status = false
                };

            ctx.ServiceRequests.Add(request);
            await ctx.SaveChangesAsync();

            var msg = $"Service request created under contract '{contract.Title}'.";
            await Notify(msg, contract.Id, contract.Status);
            return new ServiceRequestResponseDto { ServiceRequest = request, Message = msg, Status = true };
        }

        // Retrieves all service requests associated with a specific contract. The method returns a list of service requests that belong to the contract identified by the provided contract ID.
        // Each service request in the list includes its details and is linked to the contract information. If no service requests are found for the given contract ID, the method returns an empty list.
        public async Task<List<ServiceRequest>> GetServiceRequestsByContract(int contractId)
        {
            using var ctx = _dbFactory.CreateDbContext();
            return await ctx.ServiceRequests
                .Where(sr => sr.ContractId == contractId)
                .Include(sr => sr.Contract)
                .ToListAsync();
        }

        // Updates the status of a specific service request identified by its ID.
        // The method takes the service request ID and the new status as parameters, and it updates the status of the corresponding service request in the database. If the service request is found and updated successfully, the method returns a response object containing the updated service request, a success status, and a message indicating the change.
        // If the service request is not found, it returns a failure response with an appropriate message.
        public async Task<ServiceRequestResponseDto> SetServiceRequestStatus(
            int requestId, ServiceRequestStatus newStatus)
        {
            using var ctx = _dbFactory.CreateDbContext();
            var sr = await ctx.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(s => s.Id == requestId);

            if (sr == null)
                return new ServiceRequestResponseDto
                {
                    Message = $"Service request {requestId} not found.",
                    Status = false
                };

            sr.Status = newStatus;
            await ctx.SaveChangesAsync();

            var msg = $"Service request #{requestId} status → {newStatus}";
            if (sr.Contract != null)
                await Notify(msg, sr.ContractId, sr.Contract.Status);

            return new ServiceRequestResponseDto { ServiceRequest = sr, Message = msg, Status = true };
        }

        // Deletes a specific service request identified by its ID, but only if its status is either Rejected or Completed. The method first retrieves the service request along with its associated contract from the database.
        // It checks if the service request exists and then validates that its status allows for deletion.
        // If the service request is not in a state that permits deletion, the method returns a failure response with a message indicating the current status and the requirement for deletion.
        // If the service request is successfully deleted, it sends a notification using SignalR to inform clients about the deletion and returns a success response containing the deleted service request and a confirmation message.
        public async Task<ServiceRequestResponseDto> DeleteServiceRequest(int requestId)
        {
            using var ctx = _dbFactory.CreateDbContext();
            var sr = await ctx.ServiceRequests
                .Include(s => s.Contract)
                .FirstOrDefaultAsync(s => s.Id == requestId);

            if (sr == null)
                return new ServiceRequestResponseDto
                {
                    Message = $"Service request {requestId} not found.",
                    Status = false
                };

            // Only allow deletion when the request is Rejected or Completed.
            if (sr.Status != ServiceRequestStatus.Rejected &&
                sr.Status != ServiceRequestStatus.Completed)
            {
                return new ServiceRequestResponseDto
                {
                    Message = $"Service request #{requestId} cannot be deleted. " +
                              $"Its current status is '{sr.Status}'. " +
                              "Only Rejected or Completed requests may be deleted.",
                    Status = false
                };
            }

            ctx.ServiceRequests.Remove(sr);
            await ctx.SaveChangesAsync();

            var msg = $"Service request #{requestId} deleted.";
            if (sr.Contract != null)
                await Notify(msg, sr.ContractId, sr.Contract.Status);

            return new ServiceRequestResponseDto { ServiceRequest = sr, Message = msg, Status = true };
        }

        // SignalR broadcast (Observer pattern)
        // This method sends a notification message to all connected clients using SignalR.
        // The notification includes a message describing the event, the ID of the contract associated with the event, and the current status of that contract. 
        private async Task Notify(string message, int contractId, ContractStatus status)
        {
            var payload = new ContractResponseDto
            {
                Message = message,
                Status = true,
                Contract = new Contract { Id = contractId, Status = status }
            };
            await _hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", payload);
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------