using System.ComponentModel.DataAnnotations;

namespace wangluoanqan.Models; // 注意你的项目名

public class User
{
    [Key]
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer";
    public int FailedAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    public bool IsMfaEnabled { get; set; } = false;
    public string? TotpSecret { get; set; }
}