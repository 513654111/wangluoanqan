using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using wangluoanqan.Data;
using wangluoanqan.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using wangluoanqan.Models;

namespace wangluoanqan.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly CryptoService _crypto;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, CryptoService crypto, IConfiguration configuration)
    {
        _context = context;
        _crypto = crypto;
        _configuration = configuration;
    }

    public async Task<(bool Success, string? Token, string? Error)> Login(string username, string password, string? totpCode)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return (false, null, "Invalid credentials");

        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            return (false, null, "Account locked. Try again later.");

        if (!_crypto.VerifyPassword(password, user.PasswordHash))
        {
            user.FailedAttempts++;
            if (user.FailedAttempts >= 5)
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            await _context.SaveChangesAsync();
            return (false, null, "Invalid credentials");
        }

        if (user.IsMfaEnabled)
        {
            if (string.IsNullOrEmpty(totpCode) || !_crypto.VerifyTotp(user.TotpSecret!, totpCode))
                return (false, null, "MFA code required or invalid");
        }

        // Reset failed attempts
        user.FailedAttempts = 0;
        user.LockoutEnd = null;
        await _context.SaveChangesAsync();

        var token = GenerateJwt(user);
        return (true, token, null);
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: "wangluoanqan",
            audience: "wangluoanqanUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}