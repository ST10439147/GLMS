using Microsoft.AspNetCore.SignalR;
using Moq;
using ST10439147_PRO7311_POE.Hubs;
using ST10439147_PRO7311_POE.Models;
using ST10439147_PRO7311_POE.Services;
using ST10439147_PRO7311_POE.Tests.Helpers;

namespace ST10439147_PRO7311_POE.Tests
{
    // Tests the ContractService business rules:
    //   - Contracts with active service requests cannot be deleted.
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

        // Seeds a contract with one service request and returns the contract ID.
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
                EndDate = DateTime.UtcNow.AddYears(1),
                SignedAgreementPath = "uploads/agreements/fake.pdf"   // has agreement → stays Active
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

        // Deletion rules
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
                    EndDate = DateTime.UtcNow.AddYears(1),
                    SignedAgreementPath = "uploads/agreements/fake.pdf"  // has agreement → stays Active
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

        // Search / filter
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
                        ServiceLevel = ServiceLvl.Economy,
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
                        EndDate = DateTime.UtcNow.AddYears(1),
                        SignedAgreementPath = "uploads/agreements/a2.pdf"
                    },
                    // One genuinely Expired contract: EndDate is in the past
                    new Contract
                    {
                        Title = "Expired1",
                        Status = ContractStatus.Expired,
                        ServiceLevel = ServiceLvl.Economy,
                        ClientId = client.Id,
                        StartDate = DateTime.UtcNow.AddYears(-2),
                        EndDate = DateTime.UtcNow.AddDays(-1)           // past end date
                    }
                );
                await ctx.SaveChangesAsync();
            }

            var results = await CreateService(dbName).SearchContracts(null, null, ContractStatus.Active);

            Assert.Equal(2, results.Count);
            Assert.All(results, c => Assert.Equal(ContractStatus.Active, c.Status));
        }

        // Auto-status: Expired when EndDate has passed
        [Fact]
        public async Task GetContract_EndDatePassed_StatusBecomesExpired()
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
                    Title = "Overdue Contract",
                    Status = ContractStatus.Active,          // stored as Active
                    ServiceLevel = ServiceLvl.Standard,
                    ClientId = client.Id,
                    StartDate = DateTime.UtcNow.AddYears(-2),
                    EndDate = DateTime.UtcNow.AddDays(-1),   // yesterday → expired
                    SignedAgreementPath = "uploads/agreements/overdue.pdf"
                };
                ctx.Contracts.Add(contract);
                await ctx.SaveChangesAsync();
                contractId = contract.Id;
            }

            var service = CreateService(dbName);
            var result = await service.GetContract(contractId);

            // Returned DTO reflects Expired
            Assert.True(result!.Status);
            Assert.Equal(ContractStatus.Expired, result.Contract!.Status);

            // Persisted value is also corrected
            using var verifyCtx = factory.CreateDbContext();
            var stored = await verifyCtx.Contracts.FindAsync(contractId);
            Assert.Equal(ContractStatus.Expired, stored!.Status);
        }

        [Fact]
        public async Task GetAllContracts_EndDatePassed_StatusBecomesExpired()
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
                    Title = "Old Contract",
                    Status = ContractStatus.Active,
                    ServiceLevel = ServiceLvl.Economy,
                    ClientId = client.Id,
                    StartDate = DateTime.UtcNow.AddYears(-3),
                    EndDate = DateTime.UtcNow.AddDays(-10),  // past end date
                    SignedAgreementPath = "uploads/agreements/old.pdf"
                });
                await ctx.SaveChangesAsync();
            }

            var service = CreateService(dbName);
            var all = await service.GetAllContracts();

            Assert.Single(all);
            Assert.Equal(ContractStatus.Expired, all[0].Status);
        }

        [Fact]
        public async Task SearchContracts_EndDatePassed_StatusBecomesExpired()
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
                    Title = "Stale Active",
                    Status = ContractStatus.Active,
                    ServiceLevel = ServiceLvl.Standard,
                    ClientId = client.Id,
                    StartDate = DateTime.UtcNow.AddYears(-2),
                    EndDate = DateTime.UtcNow.AddDays(-5),   // past end date
                    SignedAgreementPath = "uploads/agreements/stale.pdf"
                };
                ctx.Contracts.Add(contract);
                await ctx.SaveChangesAsync();
                contractId = contract.Id;
            }

            // Search with no filter — should still auto-correct and persist
            var service = CreateService(dbName);
            var results = await service.SearchContracts(null, null, null);

            Assert.Single(results);
            Assert.Equal(ContractStatus.Expired, results[0].Status);

            // Verify DB was updated
            using var verifyCtx = factory.CreateDbContext();
            var stored = await verifyCtx.Contracts.FindAsync(contractId);
            Assert.Equal(ContractStatus.Expired, stored!.Status);
        }

        // Auto-status: Draft when no signed agreement is present
        [Fact]
        public async Task GetContract_NoAgreement_StatusBecomesDraft()
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
                    Title = "No-Doc Contract",
                    Status = ContractStatus.Active,   // stored as Active
                    ServiceLevel = ServiceLvl.Standard,
                    ClientId = client.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    SignedAgreementPath = null         // no agreement uploaded
                };
                ctx.Contracts.Add(contract);
                await ctx.SaveChangesAsync();
                contractId = contract.Id;
            }

            var service = CreateService(dbName);
            var result = await service.GetContract(contractId);

            Assert.True(result!.Status);
            Assert.Equal(ContractStatus.Draft, result.Contract!.Status);

            // Verify the correction was persisted
            using var verifyCtx = factory.CreateDbContext();
            var stored = await verifyCtx.Contracts.FindAsync(contractId);
            Assert.Equal(ContractStatus.Draft, stored!.Status);
        }

        [Fact]
        public async Task GetAllContracts_NoAgreement_StatusBecomesDraft()
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
                    Title = "Unsigned",
                    Status = ContractStatus.OnHold,   // any non-Draft stored status
                    ServiceLevel = ServiceLvl.Premium,
                    ClientId = client.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    SignedAgreementPath = null         // no agreement
                });
                await ctx.SaveChangesAsync();
            }

            var all = await CreateService(dbName).GetAllContracts();

            Assert.Single(all);
            Assert.Equal(ContractStatus.Draft, all[0].Status);
        }

        // Auto-status: Expired takes priority over Draft
        [Fact]
        public async Task GetContract_EndDatePassedAndNoAgreement_StatusIsExpiredNotDraft()
        {
            // If both rules fire, Expired takes priority because the contract
            // end date is the stronger business condition.
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
                    Title = "Worst Case Contract",
                    Status = ContractStatus.Active,
                    ServiceLevel = ServiceLvl.Economy,
                    ClientId = client.Id,
                    StartDate = DateTime.UtcNow.AddYears(-2),
                    EndDate = DateTime.UtcNow.AddDays(-1),  // past end date
                    SignedAgreementPath = null               // also no agreement
                };
                ctx.Contracts.Add(contract);
                await ctx.SaveChangesAsync();
                contractId = contract.Id;
            }

            var service = CreateService(dbName);
            var result = await service.GetContract(contractId);

            Assert.True(result!.Status);
            Assert.Equal(ContractStatus.Expired, result.Contract!.Status);
        }

        // Auto-status: no change when contract is legitimately Active
        [Fact]
        public async Task GetContract_FutureDateAndHasAgreement_StatusUnchanged()
        {
            // A contract with a future end date and an uploaded agreement should
            // keep whatever status was stored — no auto-correction should fire.
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
                    Title = "Valid Contract",
                    Status = ContractStatus.Active,
                    ServiceLevel = ServiceLvl.Premium,
                    ClientId = client.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1),
                    SignedAgreementPath = "uploads/agreements/valid.pdf"
                };
                ctx.Contracts.Add(contract);
                await ctx.SaveChangesAsync();
                contractId = contract.Id;
            }

            var result = await CreateService(dbName).GetContract(contractId);

            Assert.True(result!.Status);
            Assert.Equal(ContractStatus.Active, result.Contract!.Status);
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------