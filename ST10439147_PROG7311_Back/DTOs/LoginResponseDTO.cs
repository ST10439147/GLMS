using ST10439147_PRO7311_POE_Back.Models;

namespace ST10439147_PRO7311_POE_Back.DTOs
{
    /// <summary>
    /// Returned by POST /api/auth/login on successful authentication.
    /// </summary>
    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public User? User { get; set; }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------
