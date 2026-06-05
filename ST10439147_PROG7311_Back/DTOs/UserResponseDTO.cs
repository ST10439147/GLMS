using ST10439147_PRO7311_POE_Back.Models;

namespace ST10439147_PRO7311_POE_Back.DTOs
{
    /// <summary>
    /// UserResponseDTO is a data transfer object used to encapsulate the result of user-related operations, such as creation or update.
    /// It contains the user object, a message describing the result, and a status indicating success or failure.
    /// </summary>
    public class UserResponseDTO
    {
        public User? User { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; } = false;
    }

    /// <summary>Request body for PATCH /api/users/{id} — all fields optional.</summary>
    public class PatchUserDto
    {
        /// <summary>New first name. Omit to keep the existing value.</summary>
        public string? FirstName { get; set; }
        /// <summary>New last name. Omit to keep the existing value.</summary>
        public string? LastName { get; set; }
        /// <summary>New username. Omit to keep the existing value.</summary>
        public string? UserName { get; set; }
        /// <summary>New email address. Omit to keep the existing value.</summary>
        public string? Email { get; set; }
        /// <summary>New password. Omit to keep the existing value.</summary>
        public string? Password { get; set; }
        /// <summary>New access level (1=Admin, 2=Manager, 3=Viewer). Omit to keep the existing value.</summary>
        public int? AccessLevel { get; set; }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------