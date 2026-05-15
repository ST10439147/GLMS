using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10439147_PRO7311_POE.Models
{
    // Role convention matches the Blazor Tutorial pattern:
    // AccessLevel 1 = Admin  (full CRUD)
    // AccessLevel 2 = Manager (read + limited write)
    // AccessLevel 3 = Viewer  (read-only)
    public enum UserRole
    {
        Admin = 1,
        Manager = 2,
        Viewer = 3
    }

    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        // In a production system this would be a hashed password.
        [Required, MaxLength(200)]
        public string Password { get; set; } = string.Empty;


        public int AccessLevel { get; set; } = (int)UserRole.Viewer;

        // This property is not mapped to the database and is ignored during JSON serialization.
        [NotMapped]
        [System.Text.Json.Serialization.JsonIgnore]
        public UserRole Role => (UserRole)AccessLevel;
    }
}