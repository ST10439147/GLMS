using ST10439147_PRO7311_POE.Models;

namespace GLMS.Services
{
    /// <summary>
    /// Scoped service that stores the currently authenticated user for the
    /// lifetime of the Blazor Server circuit (one session).  Mirrors the
    /// UserState pattern used in the Blazor Tutorial.
    /// </summary>
    public class UserState
    {
        public User? CurrentUser { get; set; }

        public bool IsLoggedIn => CurrentUser != null;

        public bool IsAdmin => CurrentUser?.AccessLevel == (int)UserRole.Admin;
        public bool IsManager => CurrentUser?.AccessLevel == (int)UserRole.Manager
                                || CurrentUser?.AccessLevel == (int)UserRole.Admin;
        public bool IsViewer => IsLoggedIn; // every role can view

        public string RoleLabel => CurrentUser == null
            ? "Guest"
            : CurrentUser.Role.ToString();
    }
}
