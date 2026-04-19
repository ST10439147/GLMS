using System.ComponentModel.DataAnnotations;

namespace ST10439147_PRO7311_POE.DTOs
{
    public class LoginDTO
    {
        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        // Populated after a successful login – used by controllers for access checks
        public int AccessLevel { get; set; }
    }
}