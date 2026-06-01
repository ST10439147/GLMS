using Microsoft.EntityFrameworkCore;
using ST10439147_PRO7311_POE_Back.Data;

namespace ST10439147_PRO7311_POE.Tests.Helpers
{
    // Creates a fresh, isolated in-memory AppDbContext for each test.
    // Using a unique database name per call ensures tests never share state.

    public static class DbContextHelper
    {
        public static IDbContextFactory<AppDbContext> CreateFactory(string? dbName = null)
        {
            dbName ??= Guid.NewGuid().ToString();
            return new TestDbContextFactory(dbName);
        }

        private sealed class TestDbContextFactory(string dbName) : IDbContextFactory<AppDbContext>
        {
            public AppDbContext CreateDbContext()
            {
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(dbName)
                    .Options;
                return new AppDbContext(options);
            }
        }
    }
}
// --------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------