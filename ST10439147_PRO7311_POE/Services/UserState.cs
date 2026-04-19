using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using ST10439147_PRO7311_POE.Models;

namespace GLMS.Services
{
    public class UserState
    {
        private User? _currentUser;

        public User? CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value;
        }

        public bool IsLoggedIn => _currentUser != null;
        public bool IsAdmin => _currentUser?.AccessLevel == (int)UserRole.Admin;
        public bool IsManager => _currentUser?.AccessLevel == (int)UserRole.Manager
                                || _currentUser?.AccessLevel == (int)UserRole.Admin;
        public bool IsViewer => IsLoggedIn;

        public string RoleLabel => _currentUser == null
            ? "Guest"
            : _currentUser.Role.ToString();
    }
}