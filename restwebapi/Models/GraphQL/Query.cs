using restwebapi.Entities;

namespace restwebapi.Models.GraphQL;

[QueryType]
public static class Query
{
    public static IQueryable<Message> GetMessages(AppDbContext context) => context.Messages;
}

public class Mutation
{
    public async Task<Message> AddMessageAsync(Message message, AppDbContext context)
    {
        context.Messages.Add(message);
        await context.SaveChangesAsync();
        return message;
    }
}

public record Book(string Title, Author Author);

public record Author(string Name);
