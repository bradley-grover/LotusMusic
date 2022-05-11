using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Victoria;

namespace LotusMusic.Data.Entities;

#nullable disable

public class Playlist
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    [Required]
    public string PlaylistName { get; set; }

    [Required]
    public List<LavaTrack> Tracks { get; set; } = new();
}
