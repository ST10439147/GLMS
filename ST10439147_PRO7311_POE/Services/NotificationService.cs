using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using ST10439147_PRO7311_POE.DTOs;

namespace GLMS.Services
{
    // NotificationService mirrors the tutorial's pattern exactly.
    // Registered as Scoped so one SignalR connection exists per user session.
    // Pages subscribe to OnMessageReceived to update their UI without polling.

    public class NotificationService
    {
        private readonly NavigationManager _navigation;
        private HubConnection? _hubConnection;

        public event Action<string>? OnMessageReceived;

        public NotificationService(NavigationManager navigation)
        {
            _navigation = navigation;
        }

        public async Task StartAsync()
        {
            if (_hubConnection != null)
                return;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navigation.ToAbsoluteUri("/notificationHub"))
                .WithAutomaticReconnect()
                .Build();

            // Listen for contract status updates pushed by ContractService
            _hubConnection.On<ContractResponseDto>("ReceiveStatusUpdate", (data) =>
            {
                OnMessageReceived?.Invoke(data.Message);
            });

            await _hubConnection.StartAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
                await _hubConnection.DisposeAsync();
        }
    }
}
