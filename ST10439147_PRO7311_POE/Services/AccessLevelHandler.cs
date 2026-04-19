using GLMS.Services;

namespace ST10439147_PRO7311_POE.Services
{
    /// <summary>
    /// Automatically attaches the logged-in user's access level as a request header
    /// so API controllers can authorise correctly.
    /// Registered as a DelegatingHandler on the scoped HttpClient in Program.cs.
    /// </summary>
    public class AccessLevelHandler : DelegatingHandler
    {
        private readonly UserState _userState;

        public AccessLevelHandler(UserState userState)
        {
            _userState = userState;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Always send the current user's access level (0 = not logged in)
            var level = _userState.CurrentUser?.AccessLevel ?? 0;
            request.Headers.Remove("X-Access-Level");
            request.Headers.Add("X-Access-Level", level.ToString());
            return base.SendAsync(request, cancellationToken);
        }
    }
}