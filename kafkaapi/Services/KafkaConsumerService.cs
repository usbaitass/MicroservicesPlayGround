using Confluent.Kafka;

namespace kafkaapi.Services;

public class KafkaConsumerService : BackgroundService, IKafkaConsumerService
{
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ConsumerConfig _config;
    private readonly string _defaultHostName = "localhost:9092";
    private readonly IDaprMessagingService _daprMessagingService;

    public KafkaConsumerService(
        ILogger<KafkaConsumerService> logger,
        IConfiguration configuration,
        IDaprMessagingService daprMessagingService)
    {
        _logger = logger;
        _configuration = configuration;
        var hostName = _configuration["KafkaSettings:KafkaUrl"] ?? _defaultHostName;
        _logger.LogInformation($"[{DateTime.Now:T}] HOSTNAME: {hostName}");

        _config = new ConsumerConfig
        {
            BootstrapServers = hostName,
            GroupId = "test-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _daprMessagingService = daprMessagingService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_config).Build();

        consumer.Subscribe("test-topic");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumerResult = consumer.Consume(TimeSpan.FromSeconds(5));

                if (consumerResult == null)
                {
                    continue;
                }

                _logger.LogInformation($"Consumed message '{consumerResult.Message.Value}' at: '{consumerResult.Offset}'.");

                var messageReceived = $"{consumerResult.Message.Value};KafkaAPI {DateTime.Now:O};";

                try
                {
                    _logger.LogInformation($"SEND NEXT message: {messageReceived}");

                    var response = await _daprMessagingService.SendMessageAsync(messageReceived);

                    _logger.LogInformation($"Response message from RestWeb Api: {response}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Could not send message via Dapr. Error occurred: {ex.Message}");
                    //throw; //todo handle exception later
                }
            }
            catch (OperationCanceledException ex)
            {
                // ignore
                _logger.LogError($"Error occurred: {ex.Message}");
            }
            catch (ConsumeException ex)
            {
                _logger.LogError($"Error occurred: {ex.Error.Reason}");
            }
        }
    }
}

public interface IKafkaConsumerService
{

}
