using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace rabbitmqapi.Services;

public class RabbitMqConsumer : BackgroundService, IRabbitMqConsumer
{
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _hostname = "localhost";
    private readonly string _queueName = "message-queue";
    private readonly IKafkaProducerService _kafkaProducerService;

    private IChannel? _channel;
    private IConnection? _connection;

    public RabbitMqConsumer(
        ILogger<RabbitMqConsumer> logger,
        IConfiguration configuration,
        IKafkaProducerService kafkaProducerService)
    {
        _logger = logger;
        _configuration = configuration;
        _hostname = _configuration["RabbitMqSettings:RabbitMqUrl"] ?? "localhost";
        _logger.LogInformation($"[{DateTime.Now:T}] HOSTNAME: {_hostname}");
        _kafkaProducerService = kafkaProducerService;
    }

    protected async override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await ConnectToRabbitMq(cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel!);
        consumer.ReceivedAsync += async (sender, eventArgs) =>
        {
            var body = eventArgs.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            _logger.LogInformation($"[{DateTime.Now:T}] Received message: {message}");

            try
            {
                message = $"{message};RabbitMqAPI {DateTime.Now:O};";

                await _kafkaProducerService.ProduceAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error producing message to Kafka API");
                //throw; //todo handle exception later
            }

            await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken);
        };

        await _channel!.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);

        // Keep the consumer running
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellationToken);
        }
    }

    private async Task ConnectToRabbitMq(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory() { HostName = _hostname };
        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _connection.ConnectionShutdownAsync += OnConnectionShutdown;
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        _logger.LogInformation($"Ready to receive messages from RabbitMQ queue '{_queueName}'.");
    }

    private async Task OnConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        _logger.LogInformation("RabbitMQ connection shut down. Reconnecting...");
        await Reconnect();
    }

    private async Task Reconnect()
    {
        // Simple retry loop
        while (true)
        {
            try
            {
                Thread.Sleep(5000);
                await ConnectToRabbitMq(CancellationToken.None);
                _logger.LogInformation("Reconnected to RabbitMQ");
                break;
            }
            catch
            {
                _logger.LogInformation("Retrying RabbitMQ connection...");
            }
        }
    }

    public override void Dispose()
    {
        _channel?.CloseAsync();
        _connection?.CloseAsync();
        base.Dispose();
    }
}

public interface IRabbitMqConsumer
{
}
