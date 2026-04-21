using Microsoft.AspNetCore.SignalR;
using Moq;
using ST10439147_PRO7311_POE.Hubs;
using ST10439147_PRO7311_POE.Models;
using ST10439147_PRO7311_POE.Services;
using ST10439147_PRO7311_POE.Tests.Helpers;

namespace ST10439147_PRO7311_POE.Tests
{
    // Tests the ContractService business rules:
    //   Rule 1 - Active and OnHold contracts cannot be deleted.
    //            The contract must be Expired, Draft, or Rejected first.
    //   Rule 2 - A contract can only be deleted once all its service requests
    //            are Rejected or Completed (or there are none).
    //   - SearchContracts filters correctly by status.
    //   - Contracts whose EndDate has passed are automatically set to Expired.
    //   - Contracts without a signed agreement are automatically set to Draft.
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

        // Seeds a contract with a specific status and one service request, returns the contract ID.
        private static async Task<int> SeedContractWithRequest(
            string dbName, ContractStatus contractStatus, ServiceRequestStatus srStatus)
        {
            var factory = DbContextHelper.CreateFactory(dbName);
            using var ctx = factory.CreateDbContext();

            var client = new Clients { Name = "C", Region = ClientRegion.SubSaharanAfrica };
            ctx.Clients.Add(client);
            await ctx.SaveChangesAsync();

            var contract = new Contract
            {
                Title = "Test Contract",
                Status = contractStatus,
                ServiceLevel = ServiceLvl.Standard,
                ClientId = client.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                SignedAgreementPath = "uploads/agreements/fake.pdf"
            };
            ctx.Contracts.Add(contract);
            await ctx.SaveChangesAsync();

            ctx.ServiceRequests.Add(new ServiceRequest
            {
                ContractId = contract.Id,
                Description = "SR",
                Status = srStatus
            });
            await ctx.SaveChangesAsync();
            return contract.Id;
        }

        // Seeds a contract with a specific status and no service requests, returns the contract ID.
        private static async Task<int> SeedContractNoRequests(string dbName, ContractStatus contractStatus)
        {
            var factory = DbContextHelper.CreateFactory(dbName);
            using var ctx = factory.CreateDbContext();

            var client = new Clients { Name = "C", Region = ClientRegion.SubSaharanAfrica };
            ctx.Clients.Add(client);
            await ctx.SaveChangesAsync();

            var contract = new Contract
            {
                Title = "Test Contract",
                Status = contractStatus,
                ServiceLevel = ServiceLvl.Standard,
                ClientId = client.Id,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddYears(1),
                SignedAgreementPath = "uploads/agreements/fake.pdf"
            };
            ctx.Contracts.Add(contract);
            await ctx.SaveChangesAsync();
            return contract.Id;
        }

        //  Rule 1: Active / OnHold contracts block deletion

        [Theory]
        [InlineData(ContractStatus.Active)]
        [InlineData(ContractStatus.OnHold)]
        public async Task DeleteContract_ActiveOrOnHold_Fails(ContractStatus status)
        {
            var dbName = Guid.NewGuid().ToString();
            var contractId = await SeedContractNoRequests(dbName, status);

            var result = await CreateService(dbName).DeleteContract(contractId);

            Assert.False(result!.Status);
            Assert.Contains("Expired, Draft, or Rejected", result.Message);
            Assert.Contains(status.ToString(), result.Message);
        }

        //  Rule 1: Terminal-status contracts with no service requests can be deleted 

        [Theory]
        [InlineData(ContractStatus.Expired)]
        [InlineData(ContractStatus.Draft)]
        public async Task DeleteContract_TerminalStatus_NoServiceRequests_Succeeds(ContractStatus status)
        {
            var dbName = Guid.NewGuid().ToString();
            var contractId = await SeedContractNoRequests(dbName, status);

            var result = await CreateService(dbName).DeleteContract(contractId);

            Assert.True(result!.Status);
            Assert.Contains("deleted", result.Message);
        }

        // Rule 2: Open service requests block deletion (even on terminal contracts)

        [Theory]
        [InlineData(ContractStatus.Expired, ServiceRequestStatus.Pending)]
        [InlineData(ContractStatus.Expired, ServiceRequestStatus.Approved)]
        [InlineData(ContractStatus.Draft, ServiceRequestStatus.Pending)]
        [InlineData(ContractStatus.Draft, ServiceRequestStatus.Approved)]
        public async Task DeleteContract_HasOpenServiceRequest_Fails(
            ContractStatus contractStatus, ServiceRequestStatus srStatus)
        {
            var dbName = Guid.NewGuid().ToString();
            var contractId = await SeedContractWithRequest(dbName, contractStatus, srStatus);

            var result = await CreateService(dbName).DeleteContract(contractId);

            Assert.False(result!.Status);
            Assert.Contains("Rejected or Completed", result.Message);
        }

        // ── Rule 2: Closed service requests allow deletion on terminal contracts ──

        [Theory]
        [InlineData(ContractStatus.Expired, ServiceRequestStatus.Completed)]
        [InlineData(ContractStatus.Expired, ServiceRequestStatus.Rejected)]
        [InlineData(ContractStatus.Draft, ServiceRequestStatus.Completed)]
        [InlineData(ContractStatus.Draft, ServiceRequestStatus.Rejected)]
        public async Task DeleteContract_TerminalStatus_ClosedServiceRequests_Succeeds(
            ContractStatus contractStatus, ServiceRequestStatus srStatus)
        {
            var dbName = Guid.NewGuid().ToString();
            var contractId = await SeedContractWithRequest(dbName, contractStatus, srStatus);

            var result = await CreateService(dbName).DeleteContract(contractId);

            Assert.True(result!.Status);
            Assert.Contains("deleted", result.Message);
        }

        // ── Search / filter ───────────────────────────────────────────────────────

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
                    // Two genuinely Active contracts: future end date + signed agreement
                    new Contract
                    {
                        Title = "Active1",
                        Status = ContractStatus.Active,
                        ServiceLevel = ServiceLvl.Standard,
                        ClientId = client.Id,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddYears(1),
                        SignedAgreementPath = "uploads/agreements/a1.pdf"
                    },
                    new Contract
                    {
                        Title = "Active2",
                        Status = ContractStatus.Active,
                        ServiceLevel = ServiceLvl.Premium,
                        ClientId = client.Id,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddYears(2),
                        SignedAgreementPath = "uploads/agreements/a2.pdf"
                    },
                    // One Draft contract
                    new Contract
                    {
                        Title = "Draft1",
                        Status = ContractStatus.Draft,
                        ServiceLevel = ServiceLvl.Standard,
                        ClientId = client.Id,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddYears(1)
                    });

                await ctx.SaveChangesAsync();
            }

            var service = CreateService(dbName);
            var results = await service.SearchContracts(null, null, ContractStatus.Active);

            Assert.Equal(2, results.Count);
            Assert.All(results, c => Assert.Equal(ContractStatus.Active, c.Status));
        }

        // Auto-status correction 

        [Fact]
        public async Task GetContract_ExpiredEndDate_AutoSetsExpired()
        {
            var dbName = Guid.NewGuid().ToString();
            var factory = DbContextHelper.CreateFactory(dbName);
            int contractId;

            using (var ctx = factory.CreateDbContext())
            {
                var client = new Clients { Name = "C", Region = ClientRegion.SubSaharanAfrica };
                ctx.Clients.Add(client);
                await ctx.SaveChangesAsync();

                var contract = new Contract
                {
                    Title = "Past Due",
                    Status = ContractStatus.Active,
                    ServiceLevel = ServiceLvl.Standard,
                    ClientId = client.Id,
                    StartDate = DateTime.UtcNow.AddYears(-2),
                    EndDate = DateTime.UtcNow.AddDays(-1),        // already expired
                    SignedAgreementPath = "uploads/agreements/past.pdf"
                };
                ctx.Contracts.Add(contract);
                await ctx.SaveChangesAsync();
                contractId = contract.Id;
            }

            var result = await CreateService(dbName).GetContract(contractId);

            Assert.True(result!.Status);
            Assert.Equal(ContractStatus.Expired, result.Contract!.Status);
        }

        [Fact]
        public async Task GetContract_NoSignedAgreement_AutoSetsDraft()
        {
            var dbName = Guid.NewGuid().ToString();
            var factory = DbContextHelper.CreateFactory(dbName);
            int contractId;

            using (var ctx = factory.CreateDbContext())
            {
                var client = new Clients { Name = "C", Region = ClientRegion.SubSaharanAfrica };
                ctx.Clients.Add(client);
                await ctx.SaveChangesAsync();

                var contract = new Contract
                {
                    Title = "No Agreement",
                    Status = ContractStatus.Active,
                    ServiceLevel = ServiceLvl.Standard,
                    ClientId = client.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1)
                    // SignedAgreementPath deliberately omitted
                };
                ctx.Contracts.Add(contract);
                await ctx.SaveChangesAsync();
                contractId = contract.Id;
            }

            var result = await CreateService(dbName).GetContract(contractId);

            Assert.True(result!.Status);
            Assert.Equal(ContractStatus.Draft, result.Contract!.Status);
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------