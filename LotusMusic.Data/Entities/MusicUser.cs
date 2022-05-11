using System.ComponentModel.DataAnnotations;

namespace LotusMusic.Data.Entities;

#nullable disable

public class MusicUser
{
    [Key]
    public ulong Id { get; set; }

    [Required]
    public List<Playlist> Playlists { get; set; } = new List<Playlist>();
}
