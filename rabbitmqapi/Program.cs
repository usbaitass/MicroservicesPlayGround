using rabbitmqapi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<RabbitMqConsumer>();

var app = builder.Build();

app.MapGet("/", () => "RabbitMQ API is up and running!");

app.Run();
