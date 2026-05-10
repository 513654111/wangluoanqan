using wangluoanqan.Data;
using wangluoanqan.Models;
using System.Security.Cryptography;
using System.Text;

namespace wangluoanqan.Services;

public class AuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogAudit(string action, string details)
    {
        var log = new AuditLog
        {
            Action = action,
            Details = details,
            Timestamp = DateTime.UtcNow
        };
        log.Hash = ComputeHash(log);
        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public bool VerifyLogIntegrity(AuditLog log)
    {
        var computedHash = ComputeHash(log);
        return log.Hash == computedHash;
    }

    private string ComputeHash(AuditLog log)
    {
        string raw = $"{log.Action}|{log.Details}|{log.Timestamp:O}";
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
        return Convert.ToBase64String(bytes);
    }
}