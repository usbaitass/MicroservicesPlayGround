using grpcapi.Services;

var builder = WebApplication.CreateBuilder(args);

// Allow HTTP/2 without TLS
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(System.Net.IPAddress.Any, 5002, o =>
    {
        o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
        o.UseHttps();
    });
});

builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Services.AddScoped<IWebSocketMessageService, WebSocketMessageService>();

var app = builder.Build();

app.MapGrpcService<MessageService>();

app.MapGet("/", () => "gRPC service is up and running!");

app.Run();
