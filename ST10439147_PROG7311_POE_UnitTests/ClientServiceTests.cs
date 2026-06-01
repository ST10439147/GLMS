using ST10439147_PRO7311_POE_Back.Models;
using ST10439147_PRO7311_POE_Back.Services;
using ST10439147_PRO7311_POE.Tests.Helpers;
using static ST10439147_PRO7311_POE_Back.Models.Contract;

namespace ST10439147_PRO7311_POE.Tests
{
    // Tests the ClientService business rules:
    //   Rule 1 – A client can only be deleted when ALL contracts are in a terminal status:
    //             Expired, Draft, or Rejected. Active and OnHold contracts block deletion.
    //   Rule 2 – Even for contracts in a terminal status, all service requests on that
    //             contract must be Rejected or Completed before the client can be deleted.
    public class ClientServiceTests
    {
        private static ClientService CreateService(string? dbName = null)
            => new ClientService(DbContextHelper.CreateFactory(dbName));

        // Seeds a client with one contract (no service requests).
        private static async Task<int> SeedClientWithContract(string dbName, ContractStatus status)
        {
            var factory = DbContextHelper.CreateFactory(dbName);
            using var ctx = factory.CreateDbContext();

            var client = new Clients { Name = "Test Client", Region = ClientRegion.SubSaharanAfrica, ContactDetails = "test@test.com" };
            ctx.Clients.Add(client);
            await ctx.SaveChangesAsync();

            ctx.Contracts.Add(new Contract
            {
                Title = $"Contract ({status})",
                Status = status,
                ServiceLevel = ServiceLvl.Standard,
                ClientId = client.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1)
            });
            await ctx.SaveChangesAsync();
            return client.Id;
        }

        // Seeds a client with one contract and one service request on that contract.
        private static async Task<int> SeedClientWithContractAndServiceRequest(
            string dbName, ContractStatus contractStatus, ServiceRequestStatus srStatus)
        {
            var factory = DbContextHelper.CreateFactory(dbName);
            using var ctx = factory.CreateDbContext();

            var client = new Clients { Name = "Test Client", Region = ClientRegion.SubSaharanAfrica, ContactDetails = "test@test.com" };
            ctx.Clients.Add(client);
            await ctx.SaveChangesAsync();

            var contract = new Contract
            {
                Title = $"Contract ({contractStatus})",
                Status = contractStatus,
                ServiceLevel = ServiceLvl.Standard,
                ClientId = client.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1)
            };
            ctx.Contracts.Add(contract);
            await ctx.SaveChangesAsync();

            ctx.ServiceRequests.Add(new ServiceRequest
            {
                ContractId = contract.Id,
                Description = "Test service request",
                Status = srStatus
            });
            await ctx.SaveChangesAsync();
            return client.Id;
        }

        // ── Basic CRUD ──────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateClient_ValidData_ReturnsSuccess()
        {
            var client = new Clients { Name = "New Client", Region = ClientRegion.SubSaharanAfrica, ContactDetails = "new@test.com" };

            var result = await CreateService().CreateClient(client);

            Assert.True(result.Status);
            Assert.NotNull(result.Client);
            Assert.True(result.Client!.Id > 0);
        }

        [Fact]
        public async Task DeleteClient_NoContracts_Succeeds()
        {
            var dbName = Guid.NewGuid().ToString();
            using (var ctx = DbContextHelper.CreateFactory(dbName).CreateDbContext())
            {
                ctx.Clients.Add(new Clients { Name = "Empty Client", Region = ClientRegion.SubSaharanAfrica });
                await ctx.SaveChangesAsync();
            }

            var service = CreateService(dbName);
            var clients = await service.GetAllClients();
            var result = await service.DeleteClient(clients.First().Id);

            Assert.True(result.Status);
        }

        // ── Rule 1: Active / OnHold contracts block deletion ────────────────────

        [Theory]
        [InlineData(ContractStatus.Active)]
        [InlineData(ContractStatus.OnHold)]
        public async Task DeleteClient_HasActiveOrOnHoldContract_Fails(ContractStatus status)
        {
            var dbName = Guid.NewGuid().ToString();
            var clientId = await SeedClientWithContract(dbName, status);

            var result = await CreateService(dbName).DeleteClient(clientId);

            Assert.False(result.Status);
            Assert.Contains("Expired, Draft, or Rejected", result.Message);
        }

        //  Rule 1: Expired, Draft, and Rejected contracts allow deletion (no service requests) ──

        [Theory]
        [InlineData(ContractStatus.Expired)]
        [InlineData(ContractStatus.Draft)]
        public async Task DeleteClient_HasTerminalContractNoServiceRequests_Succeeds(ContractStatus status)
        {
            var dbName = Guid.NewGuid().ToString();
            var clientId = await SeedClientWithContract(dbName, status);

            var result = await CreateService(dbName).DeleteClient(clientId);

            Assert.True(result.Status);
            Assert.Contains("deleted", result.Message);
        }

        // ── Rule 2: Terminal contracts with open service requests block deletion ─

        [Theory]
        [InlineData(ContractStatus.Expired, ServiceRequestStatus.Pending)]
        [InlineData(ContractStatus.Expired, ServiceRequestStatus.Approved)]
        [InlineData(ContractStatus.Draft, ServiceRequestStatus.Pending)]
        [InlineData(ContractStatus.Draft, ServiceRequestStatus.Approved)]
        public async Task DeleteClient_ContractHasOpenServiceRequest_Fails(
            ContractStatus contractStatus, ServiceRequestStatus srStatus)
        {
            var dbName = Guid.NewGuid().ToString();
            var clientId = await SeedClientWithContractAndServiceRequest(dbName, contractStatus, srStatus);

            var result = await CreateService(dbName).DeleteClient(clientId);

            Assert.False(result.Status);
            Assert.Contains("service request", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ── Rule 2: Terminal contracts with all service requests closed allow deletion ──

        [Theory]
        [InlineData(ContractStatus.Expired, ServiceRequestStatus.Completed)]
        [InlineData(ContractStatus.Expired, ServiceRequestStatus.Rejected)]
        [InlineData(ContractStatus.Draft, ServiceRequestStatus.Completed)]
        [InlineData(ContractStatus.Draft, ServiceRequestStatus.Rejected)]
        public async Task DeleteClient_ContractHasOnlyClosedServiceRequests_Succeeds(
            ContractStatus contractStatus, ServiceRequestStatus srStatus)
        {
            var dbName = Guid.NewGuid().ToString();
            var clientId = await SeedClientWithContractAndServiceRequest(dbName, contractStatus, srStatus);

            var result = await CreateService(dbName).DeleteClient(clientId);

            Assert.True(result.Status);
            Assert.Contains("deleted", result.Message);
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------