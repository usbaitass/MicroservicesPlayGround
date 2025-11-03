using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using restwebapi;
using restwebapi.Services;
using restwebapi.UseCases;
using reswebapi;
using StackExchange.Redis;

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

#region setup Redis

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    return ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]!);
});

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
builder.Services.AddScoped<IGetAllMessagesUseCase, GetAllMessagesUseCase>();
builder.Services.AddScoped<ISaveMessageUseCase, SaveMessageUseCase>();

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
    IMessageGrpcService messageGrpcService,
    CancellationToken cancellationToken) =>
{
    var result = await messageGrpcService.SendMessageAsync(msg, cancellationToken);

    return Results.Ok(new
    {
        Status = "Received"
    });
}).WithName("SendMessage");

app.MapPost("/receive-message", async ([FromBody] string message,
    ISaveMessageUseCase saveMessageUseCase,
    CancellationToken cancellationToken) =>
{
    var messageDto = await saveMessageUseCase.ExecuteAsync(message, cancellationToken);

    return Results.Ok(messageDto);
}).WithName("ReceiveMessage");

app.MapGet("/messages", async (
    IGetAllMessagesUseCase getAllMessagesUseCase,
    CancellationToken cancellationToken) =>
{
    var messages = await getAllMessagesUseCase.ExecuteAsync(cancellationToken);

    return Results.Ok(messages);
}).WithName("GetMessages");

#endregion

app.RunWithGraphQLCommands(args);
