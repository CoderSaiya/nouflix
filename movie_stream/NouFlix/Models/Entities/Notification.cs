using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NouFlix.Models.Entities;

public class Notification
{
    [Key]
    public int Id { get; set; }
    [MaxLength(256)]
    public string Message { get; set; } = "";
    public bool? IsRead { get; set; } = false;
    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    [Required]
    public Guid SenderId { get; set; }

    [ForeignKey("SenderId")]
    public User? Sender { get; set; }

    [Required]
    public Guid ReceiverId { get; set; }

    [ForeignKey("ReceiverId")]
    public User? Receiver { get; set; }
}