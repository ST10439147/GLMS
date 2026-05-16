using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using ST10439147_PRO7311_POE_Back.DTOs;

namespace ST10439147_PRO7311_POE_Back.Services
{
    // NotificationService mirrors the tutorial's pattern exactly.
    // Registered as Scoped so one SignalR connection exists per user session.
    // Pages subscribe to OnMessageReceived to update their UI without polling.
    // The service is disposable to ensure the SignalR connection is properly closed when the user leaves the app.

    public class NotificationService
    {
        private readonly NavigationManager _navigation;
        private HubConnection? _hubConnection;

        public event Action<string>? OnMessageReceived;

        public NotificationService(NavigationManager navigation)
        {
            _navigation = navigation;
        }

        // StartAsync initializes the SignalR connection and sets up a listener for "ReceiveStatusUpdate" messages.
        // When a message is received, it invokes the OnMessageReceived event, allowing any subscribed components to react to the update.
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

        // DisposeAsync ensures that the SignalR connection is properly closed when the service is disposed.
        // This prevents potential memory leaks or dangling connections.
        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
                await _hubConnection.DisposeAsync();
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------