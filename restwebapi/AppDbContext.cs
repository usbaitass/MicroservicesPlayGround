using Microsoft.EntityFrameworkCore;
using restwebapi.Entities;

namespace restwebapi;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Message> Messages => Set<Message>();
}
