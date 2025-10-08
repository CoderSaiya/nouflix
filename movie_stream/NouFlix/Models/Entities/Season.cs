using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NouFlix.Models.Entities;

public class Season
{
    [Key] public int Id { get; set; }
    [Required] public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    [Required] public int Number { get; set; } // 1,2,3...
    [MaxLength(128)] public string Title { get; set; } = "";
    public int? Year { get; set; }
    [NotMapped] public int Count => Episodes.Count;
    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}