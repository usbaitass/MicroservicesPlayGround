using kafkaapi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<KafkaConsumerService>();
builder.Services.AddScoped<IDaprMessagingService, DaprMessagingService>();

var app = builder.Build();

app.MapGet("/", () => "Kafka API is up and running...");

app.Run();
