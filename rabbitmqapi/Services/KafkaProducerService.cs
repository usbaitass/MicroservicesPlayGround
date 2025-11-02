using Confluent.Kafka;

namespace rabbitmqapi.Services;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ProducerConfig _config;
    private readonly string _topic = "test-topic";
    private readonly string _defaultHostName = "localhost:9092";

    public KafkaProducerService(
        ILogger<KafkaProducerService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        var hostName = _configuration["KafkaSettings:KafkaUrl"] ?? _defaultHostName;
        _logger.LogInformation($"[{DateTime.Now:T}] HOSTNAME: {hostName}");
        _config = new ProducerConfig
        {
            BootstrapServers = hostName,
            AllowAutoCreateTopics = true,
            Acks = Acks.All
        };
    }

    public async Task ProduceAsync(string message, CancellationToken cancellationToken)
    {
        using var producer = new ProducerBuilder<Null, string>(_config).Build();

        try
        {
            var deliveryResult = await producer.ProduceAsync(
                _topic,
                new Message<Null, string>
                {
                    Value = message
                },
                cancellationToken);
            _logger.LogInformation($"Delivered message to KAFKA, topic {_topic}, partition {deliveryResult.Partition}, offset {deliveryResult.Offset}, message: {deliveryResult.Value}");
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.LogError($"Failed to produce message to topic {_topic}: {ex.Error.Reason}");
        }

        producer.Flush(cancellationToken);
    }
}

public interface IKafkaProducerService
{
    Task ProduceAsync(string message, CancellationToken cancellationToken);
}
