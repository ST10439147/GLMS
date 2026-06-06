using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE_Back.Models;

namespace ST10439147_PRO7311_POE_Back.Extensions
{
    // Extension methods that let any controller read the user's access level
    // from the X-Access-Level request header (set by AccessLevelHandler).
    public static class ControllerExtensions
    {
        public static int GetAccessLevel(this ControllerBase controller)
        {
            if (controller.Request.Headers.TryGetValue("X-Access-Level", out var values)
                && int.TryParse(values.FirstOrDefault(), out var level))
                return level;

            return 0; // not logged in / header missing
        }

        // IsloggedIn is true if the access level is greater than 0, which means the user has some role assigned (Viewer, Manager, or Admin).
        public static bool IsLoggedIn(this ControllerBase controller) =>
            controller.GetAccessLevel() > 0;

        // IsManager is true if the access level is either Manager (2) or Admin (3), indicating that the user has managerial privileges.
        public static bool IsManager(this ControllerBase controller)
        {
            var level = controller.GetAccessLevel();
            return level == (int)UserRole.Manager || level == (int)UserRole.Admin;
        }

        // IsAdmin is true if the access level is Admin (1), indicating that the user has administrative privileges.
        public static bool IsAdmin(this ControllerBase controller) =>
            controller.GetAccessLevel() == (int)UserRole.Admin;
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------