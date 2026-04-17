using ST10439147_PRO7311_POE.Models;

namespace ST10439147_PRO7311_POE.DTOs
{
    public class UserResponseDTO
    {
        public User? User { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; } = false;
    }
}
