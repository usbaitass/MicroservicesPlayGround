using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using restwebapi.Entities;
using StackExchange.Redis;

namespace restwebapi.UseCases;

public interface ISaveMessageUseCase
{
    Task<Message> ExecuteAsync(string message, CancellationToken cancellationToken);
}

public class SaveMessageUseCase : ISaveMessageUseCase
{
    private readonly AppDbContext _context;
    private readonly ILogger<SaveMessageUseCase> _logger;
    private readonly IDatabase _redisCache;

    public SaveMessageUseCase(
        AppDbContext context,
        ILogger<SaveMessageUseCase> logger,
        IConnectionMultiplexer redis)
    {
        _context = context;
        _logger = logger;
        _redisCache = redis.GetDatabase();
    }

    public async Task<Message> ExecuteAsync(string message, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"[{DateTime.Now:T}] Received message from Kafka API: {message}");

        message += $";RestWebAPI {DateTime.Now:O};";

        var messageEntity = new Message
        {
            Content = message,
            CreatedAt = DateTime.UtcNow,
            Status = "DELIVERED"
        };

        try
        {
            await _context.Messages.AddAsync(messageEntity, cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                throw new Exception("Item is already exists.");
            }

            await _redisCache.KeyDeleteAsync("all_messages");

            string cacheKey = $"product:{messageEntity.Id}";
            string json = JsonSerializer.Serialize(messageEntity);
            await _redisCache.StringSetAsync(cacheKey, json, TimeSpan.FromMinutes(10));

            _logger.LogInformation("Added new message {Id} and updated Redis cache", messageEntity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while saving message to database!");
            //throw; //todo handle exception later
        }

        return messageEntity;
    }
}
