using Microsoft.AspNetCore.SignalR;

namespace websocketapi.Services;

public class MessagingHub : Hub, IMessagingHub
{
    private readonly ILogger<MessagingHub> _logger;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;

    public MessagingHub(ILogger<MessagingHub> logger, IRabbitMqPublisher rabbitMqPublisher)
    {
        _logger = logger;
        _rabbitMqPublisher = rabbitMqPublisher;
    }

    public async Task BroadcastMessage(string source, string data)
    {
        _logger.LogInformation($"[{DateTime.Now:T}] Received from {source}: {data}");

        var message = $"{data};WebSocketAPI {DateTime.Now:O};";

        try
        {
            await _rabbitMqPublisher.Publish(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing message to RabbitMQ");
            // throw;
        }

        await Clients.All.SendAsync("ReceiveUpdate", source, message);
    }
}

public interface IMessagingHub
{
    Task BroadcastMessage(string source, string data);
}