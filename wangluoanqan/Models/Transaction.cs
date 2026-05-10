using System.ComponentModel.DataAnnotations;

namespace wangluoanqan.Models;

public class Transaction
{
    [Key]
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Signature { get; set; }
    public int UserId { get; set; }  // who initiated
}