using System.ComponentModel.DataAnnotations;

namespace MoviePortal.Models.Entities;

public class Season
{
    [Key] public int Id { get; set; }
    [Required] public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    [Required] public int Number { get; set; } // 1,2,3...
    [MaxLength(128)] public string Title { get; set; } = "";
    public int? Year { get; set; }
    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}