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
      Console.WriteLine($"Received update from {source}: {data}");
      replyMessage = data;
    });

    // Start the connection
    await connection.StartAsync();
    Console.WriteLine("Client connected to SignalR server.");

    await connection.InvokeAsync("BroadcastMessage", "grpcApi", message);

    return replyMessage;
  }
}

public interface IWebSocketMessageService
{
  Task<string> SendWebSocketMessage(string message);
}