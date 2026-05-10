using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wangluoanqan.Data;

namespace wangluoanqan.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _context.Users.Select(u => new { u.Id, u.Username, u.Role, u.IsMfaEnabled }).ToListAsync();
        return Ok(users);
    }

    [HttpGet("auditlogs")]
    public async Task<IActionResult> GetAuditLogs()
    {
        var logs = await _context.AuditLogs.OrderByDescending(l => l.Timestamp).Take(100).ToListAsync();
        return Ok(logs);
    }
}