using ST10439147_PRO7311_POE.Models;

namespace ST10439147_PRO7311_POE.Services
{
    public class UserState
    {
        private User? _currentUser;

        // Components subscribe to this so they can call StateHasChanged()
        // the moment the user logs in or out — no polling needed.
        public event Action? OnChange;

        public User? CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnChange?.Invoke();   // notify all subscribers immediately
            }
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
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------