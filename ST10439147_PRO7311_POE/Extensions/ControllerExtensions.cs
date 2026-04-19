using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE.Models;

namespace ST10439147_PRO7311_POE.Extensions
{
    /// <summary>
    /// Extension methods that let any controller read the user's access level
    /// from the X-Access-Level request header (set by AccessLevelHandler).
    /// </summary>
    public static class ControllerExtensions
    {
        public static int GetAccessLevel(this ControllerBase controller)
        {
            if (controller.Request.Headers.TryGetValue("X-Access-Level", out var values)
                && int.TryParse(values.FirstOrDefault(), out var level))
                return level;

            return 0; // not logged in / header missing
        }

        public static bool IsLoggedIn(this ControllerBase controller) =>
            controller.GetAccessLevel() > 0;

        public static bool IsManager(this ControllerBase controller)
        {
            var level = controller.GetAccessLevel();
            return level == (int)UserRole.Manager || level == (int)UserRole.Admin;
        }

        public static bool IsAdmin(this ControllerBase controller) =>
            controller.GetAccessLevel() == (int)UserRole.Admin;
    }
}