using Microsoft.AspNetCore.Mvc;
using wangluoanqan.Services;

namespace wangluoanqan.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (success, token, error) = await _authService.Login(request.Username, request.Password, request.TotpCode);
        if (!success) return Unauthorized(new { message = error });
        return Ok(new { token });
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? TotpCode { get; set; }
}