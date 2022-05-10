using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LotusMusic.Data.Entities;

#nullable disable

public class Prefix
{
    [Key]
    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong Id { get; set; }

    [StringLength(4, MinimumLength = 1)]
    public string Value { get; set; }
}
