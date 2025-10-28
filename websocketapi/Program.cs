using websocketapi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddScoped<IMessagingHub, MessagingHub>();
builder.Services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();

var app = builder.Build();

app.MapGet("/", () => "WebSocketApi is up and running!");

app.MapHub<MessagingHub>("/updatesHub");

app.Run();
