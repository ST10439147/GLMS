using Microsoft.AspNetCore.SignalR.Client;
using ST10439147_PRO7311_POE.DTOs;

namespace ST10439147_PRO7311_POE.Services
{
    // Registered as Scoped so one SignalR connection exists per user session.
    // Connects to the backend API's notification hub (not the Blazor frontend URL).
    public class NotificationService
    {
        private readonly string _apiBaseUrl;
        private HubConnection? _hubConnection;

        public event Action<string>? OnMessageReceived;

        public NotificationService(IConfiguration configuration)
        {
            _apiBaseUrl = configuration["ApiBaseUrl"] ?? "https://localhost:7055";
        }

        public async Task StartAsync()
        {
            if (_hubConnection != null)
                return;

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{_apiBaseUrl}/notificationHub")
                .WithAutomaticReconnect()
                .Build();

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
