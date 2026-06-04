using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ST10439147_PRO7311_POE_Back.Data
{
    // Used only by EF tooling (add-migration / update-database).
    // Not involved in the running application at all.
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var connectionString = "Server=(localdb)\\mssqllocaldb;Database=ST10439147_GLMS_DB;Integrated Security=True;MultipleActiveResultSets=true";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------