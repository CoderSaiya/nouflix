using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NouFlix.Models.Entities;
public class Genre
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }

    [NotMapped] public int MovieCount => MovieGenres.Count;

    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
}
