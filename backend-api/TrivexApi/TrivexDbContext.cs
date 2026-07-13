using Microsoft.EntityFrameworkCore;

public class TrivexDbContext : DbContext
{
    public TrivexDbContext(DbContextOptions<TrivexDbContext> options) : base(options)
    {
    }

    public DbSet<Ticket> Tickets { get; set; }
}