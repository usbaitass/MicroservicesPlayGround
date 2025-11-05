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

    public async Task<MessageDto> SendMessageAsync(MessageDto msg, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"[{DateTime.Now:T}] Received message: {msg.Content}");

        msg.Content += $";RestWebAPI {DateTime.Now:O};";

        var grpcRequest = new MessageRequest
        {
            Content = msg.Content ?? string.Empty
        };

        string grpcReplyConfirmation;
        try
        {
            var grpcReply = await _grpcClient.SendMessageAsync(grpcRequest, cancellationToken: cancellationToken);
            grpcReplyConfirmation = grpcReply.Confirmation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while logging message content");
            throw;
        }

        return new MessageDto
        {
            Content = grpcReplyConfirmation
        };
    }
}

public interface IMessageGrpcService
{
    Task<MessageDto> SendMessageAsync(MessageDto msg, CancellationToken cancellationToken);
}

public class MessageDto()
{
    public string? Content { get; set; }
}
