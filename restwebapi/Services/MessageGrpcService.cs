using reswebapi;

namespace restwebapi.Services;

public class MessageGrpcService : IMessageGrpcService
{
    private readonly Messenger.MessengerClient _grpcClient;
    private readonly ILogger<MessageGrpcService> _logger;

    public MessageGrpcService(Messenger.MessengerClient grpcClient,
        ILogger<MessageGrpcService> logger)
    {
        _grpcClient = grpcClient;
        _logger = logger;
    }

    public async Task<Message> SendMessageAsync(Message msg, CancellationToken cancellationToken)
    {
        var grpcRequest = new MessageRequest
        {
            Content = msg.content ?? string.Empty
        };

        string grpcReplyConfirmation = string.Empty;

        try
        {
            var grpcReply = await _grpcClient.SendMessageAsync(grpcRequest, cancellationToken: cancellationToken);
            grpcReplyConfirmation = grpcReply.Confirmation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while logging message content");
            //throw;
        }
        
        return new Message
        {
            content = grpcReplyConfirmation
        };
    }
}

public interface IMessageGrpcService
{
    Task<Message> SendMessageAsync(Message msg, CancellationToken cancellationToken);
}

public class Message()
{
    public string? content { get; set; }
}