using Confluent.Kafka;

namespace kafkaapi.Services
{
    public class KafkaConsumerService : BackgroundService, IKafkaConsumerService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly ConsumerConfig _config;

        public KafkaConsumerService(ILogger<KafkaConsumerService> logger)
        {

            _logger = logger;
            _config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "test-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
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

            return Task.CompletedTask;
        }
    }

    public interface IKafkaConsumerService
    {

    }
}