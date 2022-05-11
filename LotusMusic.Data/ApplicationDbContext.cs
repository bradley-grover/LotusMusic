using LotusMusic.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Victoria;


#nullable disable

namespace LotusMusic.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        Database.EnsureCreated();
        
    }
    public DbSet<MusicUser> MusicUsers { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<LavaTrack> Tracks { get; set; }
}
