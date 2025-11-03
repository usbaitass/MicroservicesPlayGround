using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restwebapi;
using restwebapi.Entities;
using restwebapi.Services;
using reswebapi;

var builder = WebApplication.CreateBuilder(args);

#region grpc client setup

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

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

#region setup database

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

#endregion

#region configure GraphQL

builder.Services
    .AddGraphQLServer()
    .RegisterDbContextFactory<AppDbContext>()
    .AddTypes();

#endregion

# region register services

builder.Services.AddOpenApi();

builder.Services.AddScoped<IMessageGrpcService, MessageGrpcService>();

#endregion

#region configure CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4202")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

#endregion

var app = builder.Build();

#region  Configure the request pipeline

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapSwaggerUI();
}

app.MapGraphQL();

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

#endregion

#region HTTP rest endpoints

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
        catch (DbUpdateException)
        {
            throw new Exception("Item is already exists.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error while saving message to database!");
        //throw; //todo handle exception later
    }

    return Results.Ok(message);
}).WithName("ReceiveMessage");

app.MapGet("/messages", (
    [FromServices] ILogger<Program> logger,
    AppDbContext context,
    CancellationToken cancellationToken) =>
{
    var result = context.Messages;

    return Results.Ok(result);
}).WithName("GetMessages");

#endregion

app.RunWithGraphQLCommands(args);
