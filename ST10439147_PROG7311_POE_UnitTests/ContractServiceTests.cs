using Microsoft.AspNetCore.SignalR;
using Moq;
using ST10439147_PRO7311_POE.Hubs;
using ST10439147_PRO7311_POE.Models;
using ST10439147_PRO7311_POE.Services;
using ST10439147_PRO7311_POE.Tests.Helpers;

namespace ST10439147_PRO7311_POE.Tests
{ }
    // Tests the ContractService business rules:
    //   - Contracts with active service requests cannot be deleted.
    //   - SearchContracts filters correctly by status.
    public class ContractServiceTests
    {
        private static ContractService CreateService(string? dbName = null)
        {
            var factory = DbContextHelper.CreateFactory(dbName);
            var hubMock = new Mock<IHubContext<NotificationHub>>();
            var clientsMock = new Mock<IHubClients>();
            var clientProxyMock = new Mock<IClientProxy>();

            hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
            clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);
            clientProxyMock
                .Setup(p => p.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            return new ContractService(hubMock.Object, factory);
        }

        private static async Task<int> SeedContractWithRequest(string dbName, ServiceRequestStatus srStatus)
        {
            var factory = DbContextHelper.CreateFactory(dbName);
            using var ctx = factory.CreateDbContext();

            var client = new Clients { Name = "C", Region = ClientRegion.SubSaharanAfrica };
            ctx.Clients.Add(client);
            await ctx.SaveChangesAsync();

            var contract = new Contract
            {
                Title = "Blocked Contract",
                Status = ContractStatus.Active,
                ServiceLevel = ServiceLvl.Standard,
                ClientId = client.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1)
            };
            ctx.Contracts.Add(contract);
            await ctx.SaveChangesAsync();

            ctx.ServiceRequests.Add(new ServiceRequest { ContractId = contract.Id, Description = "SR", Status = srStatus });
            await ctx.SaveChangesAsync();
            return contract.Id;
        }

        [Fact]
        public async Task DeleteContract_NoServiceRequests_Succeeds()
        {
            var dbName = Guid.NewGuid().ToString();
            var factory = DbContextHelper.CreateFactory(dbName);
            using (var ctx = factory.CreateDbContext())
            {
                var client = new Clients { Name = "C", Region = ClientRegion.SubSaharanAfrica };
                ctx.Clients.Add(client);
                await ctx.SaveChangesAsync();

                ctx.Contracts.Add(new Contract
                {
                    Title = "Clean Contract",
                    Status = ContractStatus.Active,
                    ServiceLevel = ServiceLvl.Standard,
                    ClientId = client.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1)
                });
                await ctx.SaveChangesAsync();
            }

            var service = CreateService(dbName);
            var all = await service.GetAllContracts();
            var result = await service.DeleteContract(all.First().Id);

            Assert.True(result!.Status);
        }

        [Theory]
        [InlineData(ServiceRequestStatus.Pending)]
        [InlineData(ServiceRequestStatus.Approved)]
        public async Task DeleteContract_HasActiveRequest_Fails(ServiceRequestStatus srStatus)
        {
            var dbName = Guid.NewGuid().ToString();
            var contractId = await SeedContractWithRequest(dbName, srStatus);

            var result = await CreateService(dbName).DeleteContract(contractId);

            Assert.False(result!.Status);
            Assert.Contains("Rejected or Completed", result.Message);
        }

        [Fact]
        public async Task SearchContracts_ByStatus_ReturnsOnlyMatching()
        {
            var dbName = Guid.NewGuid().ToString();
            var factory = DbContextHelper.CreateFactory(dbName);

            using (var ctx = factory.CreateDbContext())
            {
                var client = new Clients { Name = "C", Region = ClientRegion.SubSaharanAfrica };
                ctx.Clients.Add(client);
                await ctx.SaveChangesAsync();

                ctx.Contracts.AddRange(
                    new Contract { Title = "Active1", Status = ContractStatus.Active, ServiceLevel = ServiceLvl.Economy, ClientId = client.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1) },
                    new Contract { Title = "Expired1", Status = ContractStatus.Expired, ServiceLevel = ServiceLvl.Economy, ClientId = client.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1) },
                    new Contract { Title = "Active2", Status = ContractStatus.Active, ServiceLevel = ServiceLvl.Premium, ClientId = client.Id, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddYears(1) }
                );
                await ctx.SaveChangesAsync();
            }

            var results = await CreateService(dbName).SearchContracts(null, null, ContractStatus.Active);

            Assert.Equal(2, results.Count);
            Assert.All(results, c => Assert.Equal(ContractStatus.Active, c.Status));
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------