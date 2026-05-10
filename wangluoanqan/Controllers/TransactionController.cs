using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wangluoanqan.Data;
using wangluoanqan.Services;
using System.Security.Claims;

namespace wangluoanqan.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CryptoService _crypto;
    private readonly AuditService _audit;

    public TransactionController(AppDbContext context, CryptoService crypto, AuditService audit)
    {
        _context = context;
        _crypto = crypto;
        _audit = audit;
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        // Input validation
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null) return Unauthorized();
        int userId = int.Parse(userIdClaim);

        // Build transaction (parameterized query via EF)
        var transaction = new Models.Transaction
        {
            AccountNumber = request.AccountNumber,
            Amount = request.Amount,
            Description = request.Description,
            Timestamp = DateTime.UtcNow,
            UserId = userId
        };

        // Sign transaction
        string payload = $"{transaction.AccountNumber}|{transaction.Amount}|{transaction.Timestamp:O}";
        transaction.Signature = _crypto.SignTransaction(payload);

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        await _audit.LogAudit("Transfer", $"User {userId} transferred {request.Amount} to {request.AccountNumber}");

        return Ok(new { message = "Transaction completed", signature = transaction.Signature });
    }
}

public class TransferRequest
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(20), System.ComponentModel.DataAnnotations.RegularExpression(@"^[a-zA-Z0-9\-]+$")]
    public string AccountNumber { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Range(0.01, 1000000)]
    public decimal Amount { get; set; }
    [System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string? Description { get; set; }
}