using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ST10439147_PRO7311_POE.Data
{
    /// <summary>
    /// Used only by EF tooling (add-migration / update-database).
    /// Not involved in the running application at all.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Resolve the absolute path to the Data folder at design time
            var dataFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            var mdfPath = Path.Combine(dataFolder, "ST10439147_PRO7311_POE.mdf");

            // Ensure the Data folder exists
            Directory.CreateDirectory(dataFolder);

            var connectionString = $"Server=(localdb)\\mssqllocaldb;" +
                                   $"AttachDbFilename={mdfPath};" +
                                   $"Database=ST10439147_GLMS_DB;" +
                                   $"Integrated Security=True;" +
                                   $"MultipleActiveResultSets=true";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}