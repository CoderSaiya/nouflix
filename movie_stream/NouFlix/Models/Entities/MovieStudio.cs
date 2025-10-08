﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NouFlix.Models.Entities
{
    public class MovieStudio
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        [ForeignKey("MovieId")]
        public Movie Movie { get; set; } = null!;

        [Required]
        public int StudioId { get; set; }

        [ForeignKey("StudioId")]
        public Studio Studio { get; set; } = null!;
    }
}
