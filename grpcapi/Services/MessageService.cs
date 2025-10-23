using Grpc.Core;

namespace grpcapi.Services;

public class MessageService : Messenger.MessengerBase
{
    private readonly ILogger<MessageService> _logger;
    public MessageService(ILogger<MessageService> logger)
    {
        _logger = logger;
    }

    public override Task<MessageReply> SendMessage(MessageRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Received message from: {Content}", request.Content);

        return Task.FromResult(new MessageReply
        {
            Confirmation = $"{request.Content};GrpcAPI {DateTime.Now:O};"
        });
    }
}