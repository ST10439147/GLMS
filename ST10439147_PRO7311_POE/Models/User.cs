namespace ST10439147_PRO7311_POE.Models
{
    public enum UserRole
    {
        Admin = 1,
        Manager = 2,
        Viewer = 3
    }

    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int AccessLevel { get; set; } = (int)UserRole.Viewer;
        public UserRole Role => (UserRole)AccessLevel;
    }
}
