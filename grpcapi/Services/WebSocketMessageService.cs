using Microsoft.AspNetCore.SignalR.Client;

namespace grpcapi.Services;

public class WebSocketMessageService : IWebSocketMessageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MessageService> _logger;

    public WebSocketMessageService(IConfiguration configuration,
      ILogger<MessageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> SendWebSocketMessage(string message)
    {
        // Configure SignalR connection to the server hub
        var connection = new HubConnectionBuilder()
          .WithUrl(new Uri($"{_configuration["WebSocketApiSettings:WebSocketApiUrl"]!}/updatesHub"))
          .WithAutomaticReconnect()
          .Build();

        string replyMessage = string.Empty;

        // Handle incoming updates from server
        connection.On<string, string>("ReceiveUpdate", (source, data) =>
        {
            _logger.LogInformation($"Received update from {source}: {data}");
            replyMessage = data;
        });

        // Start the connection
        await connection.StartAsync();
        _logger.LogInformation("SignalR connection started.");

        var res = await connection.InvokeAsync("BroadcastMessage", "grpcApi", message).ContinueWith(task =>
        {
            Task.Delay(1000).Wait(); // wait for a second to receive the message
            return task;
        });

        // Stop the connection
        await connection.StopAsync();
        _logger.LogInformation("SignalR connection stopped.");

        return replyMessage;
    }
}

public interface IWebSocketMessageService
{
    Task<string> SendWebSocketMessage(string message);
}