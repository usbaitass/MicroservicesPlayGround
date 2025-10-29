using Grpc.Core;

namespace grpcapi.Services;

public class MessageService : Messenger.MessengerBase
{
    private readonly ILogger<MessageService> _logger;
    private readonly IWebSocketMessageService _webSocketMessageService;

    public MessageService(ILogger<MessageService> logger,
        IWebSocketMessageService webSocketMessageService)
    {
        _logger = logger;
        _webSocketMessageService = webSocketMessageService;
    }

    public override async Task<MessageReply> SendMessage(MessageRequest request, ServerCallContext context)
    {
        _logger.LogInformation($"[{DateTime.Now:T}] Received message: {request.Content}");

        var message = $"{request.Content};GrpcAPI {DateTime.Now:O};";

        var result = string.Empty;

        try
        {
            result = await _webSocketMessageService.SendWebSocketMessage(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending WebSocket message");
            //throw;
        }

        return await Task.FromResult(new MessageReply
        {
            Confirmation = result
        });
    }
}