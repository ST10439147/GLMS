namespace ST10439147_PRO7311_POE.Services
{
    // Automatically attaches the logged-in user's access level as a request header so API controllers can authorise correctly.
    // Registered as a DelegatingHandler on the scoped HttpClient in Program.cs.    
    public class AccessLevelHandler : DelegatingHandler
    {
        private readonly UserState _userState;

        public AccessLevelHandler(UserState userState)
        {
            _userState = userState;
        }

        // This method is called for every outgoing HTTP request. It adds the "X-Access-Level" header with the current user's access level before sending the request.
        // If the user is not logged in, it defaults to 0. This allows API controllers to check the access level and authorize requests accordingly.
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
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------