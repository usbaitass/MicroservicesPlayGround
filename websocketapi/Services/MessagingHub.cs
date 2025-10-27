using Microsoft.AspNetCore.SignalR;

namespace websocketapi.Services;

public class MessagingHub : Hub, IMessagingHub
{
  private readonly ILogger<MessagingHub> _logger;

  public MessagingHub(ILogger<MessagingHub> logger)
  {
    _logger = logger;
  }

  public async Task BroadcastMessage(string source, string data)
  {
    _logger.LogInformation($"[{DateTime.Now:T}] Received from {source}: {data}");

    var message = $"{data};WebSocketAPI {DateTime.Now:O};";

    await Clients.All.SendAsync("ReceiveUpdate", source, message);
  }
}

public interface IMessagingHub
{
  Task BroadcastMessage(string source, string data);
}