using System.Text;
using RabbitMQ.Client;

namespace websocketapi.Services
{
    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly ILogger<RabbitMqPublisher> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _hostname = "localhost";
        private readonly string _queueName = "message-queue";

        public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _hostname = _configuration["RabbitMqSettings:RabbitMqUrl"] ?? "localhost";
            _logger.LogInformation($"[{DateTime.Now:T}] HOSTNAME: {_hostname}");
        }
        
        public async Task Publish(string message)
        {
            var factory = new ConnectionFactory() { HostName = _hostname };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: _queueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _queueName,
                body: body);

            _logger.LogInformation($"Sent: {message}");
        }
    }

    public interface IRabbitMqPublisher
    {
        Task Publish(string message);
    }
}