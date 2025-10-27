using websocketapi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

builder.Services.AddScoped<IMessagingHub, MessagingHub>();

var app = builder.Build();

app.MapGet("/", () => "WebSocketApi is up and running!");

app.MapHub<MessagingHub>("/updatesHub");

app.Run();
