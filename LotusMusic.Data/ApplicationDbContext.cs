using LotusMusic.Data.Entities;
using Microsoft.EntityFrameworkCore;


#nullable disable

namespace LotusMusic.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<Prefix> Prefixes { get; set; }
}
