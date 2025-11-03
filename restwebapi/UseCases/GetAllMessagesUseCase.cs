using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using restwebapi.Entities;
using StackExchange.Redis;

namespace restwebapi.UseCases;

public interface IGetAllMessagesUseCase
{
    Task<IList<Message>> ExecuteAsync(CancellationToken cancellationToken);
}

public class GetAllMessagesUseCase : IGetAllMessagesUseCase
{
    private readonly AppDbContext _context;
    private readonly ILogger<GetAllMessagesUseCase> _logger;
    private readonly IDatabase _redisCache;

    public GetAllMessagesUseCase(
        AppDbContext context,
        ILogger<GetAllMessagesUseCase> logger,
        IConnectionMultiplexer redis)
    {
        _context = context;
        _logger = logger;
        _redisCache = redis.GetDatabase();
    }

    public async Task<IList<Message>> ExecuteAsync(CancellationToken cancellationToken)
    {
        const string cacheKey = "all_messages";

        var cached = await _redisCache.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            _logger.LogInformation("Returning all messages from cache");
            return JsonSerializer.Deserialize<List<Message>>(cached!)!;
        }

        var messages = await _context.Messages.ToListAsync(cancellationToken);

        await _redisCache.StringSetAsync(cacheKey, JsonSerializer.Serialize(messages), TimeSpan.FromMinutes(5));

        return messages;
    }
}
