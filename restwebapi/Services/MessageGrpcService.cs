using grpcapi;

namespace restwebapi.Services;

public class MessageGrpcService : IMessageGrpcService
{
    private readonly Messenger.MessengerClient _grpcClient;

    public MessageGrpcService(Messenger.MessengerClient grpcClient)
    {
        _grpcClient = grpcClient;
    }

    public async Task<Message> SendMessageAsync(Message msg, CancellationToken cancellationToken)
    {
        var grpcRequest = new MessageRequest
        {
            Content = msg.content ?? string.Empty
        };

        var grpcReply = await _grpcClient.SendMessageAsync(grpcRequest, cancellationToken: cancellationToken);

        return new Message
        {
            content = grpcReply.Confirmation
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