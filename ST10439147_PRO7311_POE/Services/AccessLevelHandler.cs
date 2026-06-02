using System.Net.Http.Headers;

namespace ST10439147_PRO7311_POE.Services
{
    // Automatically attaches the logged-in user's access level and JWT Bearer token
    // to every outgoing API request. Registered as a DelegatingHandler in Program.cs.
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
            var level = _userState.CurrentUser?.AccessLevel ?? 0;
            request.Headers.Remove("X-Access-Level");
            request.Headers.Add("X-Access-Level", level.ToString());

            if (!string.IsNullOrEmpty(_userState.JwtToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _userState.JwtToken);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------