using ST10439147_PRO7311_POE_Back.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace ST10439147_PRO7311_POE_Back.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // Existing tables
        public DbSet<Clients> Clients => Set<Clients>();
        public DbSet<Contract> Contracts => base.Set<Models.Contract>();
        public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();

        // Auth table
        public DbSet<User> ApplicationUsers => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Existing relationships
            modelBuilder.Entity<Models.Contract>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Contracts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceRequest>()
                .HasOne(sr => sr.Contract)
                .WithMany(c => c.ServiceRequests)
                .HasForeignKey(sr => sr.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Models.Contract>()
                .Property(c => c.Title).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<Models.Contract>()
                .Property(c => c.ServiceLevel).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Clients>()
                .Property(c => c.Name).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<Clients>()
                .Property(c => c.Region).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<ServiceRequest>()
                .Property(sr => sr.Description).IsRequired().HasMaxLength(500);

            // ApplicationUsers constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.UserName).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<User>()
                .Property(u => u.Password).IsRequired().HasMaxLength(200);
            modelBuilder.Entity<User>()
                .Property(u => u.Email).IsRequired().HasMaxLength(200);

            // Seed a default Admin so the app is usable immediately
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                FirstName = "Joshua",
                LastName = "Jones",
                UserName = "admin",
                Email = "admin@glms.local",
                Password = "Admin123@",
                AccessLevel = (int)UserRole.Admin
            });
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------