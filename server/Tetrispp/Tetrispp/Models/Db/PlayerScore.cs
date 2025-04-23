using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tetrispp.Models.Db;

public class PlayerScore
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public User? User { get; set; }
    
    [Required]
    public required string RoomId { get; set; }
    
    [Required]
    public int LinesCleared { get; set; }
    
    [Required]
    public bool IsWinner { get; set; }
    
    [Required]
    public DateTime Date { get; set; } = DateTime.UtcNow;
}