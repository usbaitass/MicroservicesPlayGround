var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/messages", (Message msg) =>
{
    Console.WriteLine($"Received message from {msg.sender}: {msg.content}");
    return Results.Ok(new
    {
        Status = "Received",
        Echo = msg
    });
}).WithName("SendMessage");

app.Run();

record Message(string content, DateTime receivedAt, string sender);