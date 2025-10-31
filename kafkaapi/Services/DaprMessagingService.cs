using Dapr.Client;

namespace kafkaapi.Services;

public class DaprMessagingService : IDaprMessagingService
{
    private readonly ILogger<DaprMessagingService> _logger;

    public DaprMessagingService(ILogger<DaprMessagingService> logger) => _logger = logger;

    public async Task<string> SendMessageAsync(string message)
    {
        using var daprClient = new DaprClientBuilder().Build();

        _logger.LogInformation($"Sent message: {message}");

        var response = await daprClient.InvokeMethodAsync<object, string>(
            HttpMethod.Post, "restwebapi", $"receive-message", message);

        return response;
    }
}

public interface IDaprMessagingService
{
    Task<string> SendMessageAsync(string message);
}
