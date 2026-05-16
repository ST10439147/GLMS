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
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------