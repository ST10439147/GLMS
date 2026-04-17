using Microsoft.AspNetCore.SignalR;
using ST10439147_PRO7311_POE.Models;

namespace ST10439147_PRO7311_POE.Hubs
{
    // NotificationHub is the Subject in the Observer pattern.
    // It is kept intentionally thin - zero business logic lives here.
    // ContractService owns all business logic and calls IHubContext<NotificationHub>
    // to push updates; this hub only owns the real-time transport.

    public class NotificationHub : Hub
    {
        // SendStatusUpdate is available for any future server-side push calls.
        // Current broadcasts are triggered from ContractService via IHubContext.
        public async Task SendStatusUpdate(int contractId, string status)
        {
            await Clients.All.SendAsync("ReceiveStatusUpdate", contractId, status);
        }
    }
}
