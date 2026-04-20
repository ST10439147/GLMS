using ST10439147_PRO7311_POE.Models;
using ST10439147_PRO7311_POE.Services;
using ST10439147_PRO7311_POE.Tests.Helpers;
using static ST10439147_PRO7311_POE.Models.Contract;

namespace ST10439147_PRO7311_POE.Tests
{
    /// <summary>
    /// Tests the ClientService business rules:
    ///   - A client can only be deleted when ALL contracts are Expired.
    ///   - Draft / Active / OnHold contracts block deletion.
    /// </summary>
    public class ClientServiceTests
    {
        private static ClientService CreateService(string? dbName = null)
            => new ClientService(DbContextHelper.CreateFactory(dbName));

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

        [Theory]
        [InlineData(ContractStatus.Draft)]
        [InlineData(ContractStatus.Active)]
        [InlineData(ContractStatus.OnHold)]
        public async Task DeleteClient_HasBlockingContract_Fails(ContractStatus status)
        {
            var dbName = Guid.NewGuid().ToString();
            var clientId = await SeedClientWithContract(dbName, status);

            var result = await CreateService(dbName).DeleteClient(clientId);

            Assert.False(result.Status);
            Assert.Contains("Expired", result.Message);
        }
    }
}