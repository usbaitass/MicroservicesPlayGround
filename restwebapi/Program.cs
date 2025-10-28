using Microsoft.AspNetCore.Mvc;
using restwebapi.Services;
using reswebapi;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#region grpc client setup

builder.Services.AddGrpcClient<Messenger.MessengerClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcSettings:GrpcUrl"]!);
}).ConfigureChannel(o =>
{
    o.HttpHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

#endregion

# region register services

builder.Services.AddScoped<IMessageGrpcService, MessageGrpcService>();

#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "RestWebAPI is up and running");

app.MapPost("/messages", async ([FromServices] ILogger<Program> logger, IMessageGrpcService messageGrpcService, Message msg, CancellationToken cancellationToken) =>
{
    logger.LogInformation($"[{DateTime.Now:T}] Received message: {msg.content}");

    msg.content += $";RestWebAPI {DateTime.Now:O};";

    var result = await messageGrpcService.SendMessageAsync(msg, cancellationToken);

    return Results.Ok(new
    {
        Status = "Received"
    });
}).WithName("SendMessage");

app.Run();
