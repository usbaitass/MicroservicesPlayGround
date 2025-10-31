using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restwebapi;
using restwebapi.Entities;
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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

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

app.MapPost("/messages", async (MessageDto msg,
    [FromServices] ILogger<Program> logger,
    IMessageGrpcService messageGrpcService,
    CancellationToken cancellationToken) =>
{
    logger.LogInformation($"[{DateTime.Now:T}] Received message: {msg.Content}");

    msg.Content += $";RestWebAPI {DateTime.Now:O};";

    var result = await messageGrpcService.SendMessageAsync(msg, cancellationToken);

    return Results.Ok(new
    {
        Status = "Received"
    });
}).WithName("SendMessage");

app.MapPost("/receive-message", async ([FromBody] string message,
    [FromServices] ILogger<Program> logger,
    AppDbContext db,
    CancellationToken cancellationToken) =>
{
    logger.LogInformation($"[{DateTime.Now:T}] Received message from Kafka API: {message}");

    message += $";RestWebAPI {DateTime.Now:O};";

    var messageEntity = new Message
    {
        Content = message,
        CreatedAt = DateTime.UtcNow,
        Status = "DELIVERED"
    };

    try
    {
        await db.Messages.AddAsync(messageEntity, cancellationToken);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Item is already exists.");
            //throw new Exception("Item is already exists.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error while saving message to database!");
        //throw;
    }

    return Results.Ok(message);
}).WithName("ReceiveMessage");

app.Run();
