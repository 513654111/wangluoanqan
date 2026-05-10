using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using wangluoanqan.Services;

namespace wangluoanqan.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly VirusScanService _virusScan;

    public FileUploadController(VirusScanService virusScan)
    {
        _virusScan = virusScan;
    }

    [HttpPost("check")]
    public async Task<IActionResult> UploadCheck(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("File too large");

        // Verify magic numbers (JPEG/PNG)
        using var stream = file.OpenReadStream();
        byte[] header = new byte[8];
        await stream.ReadAsync(header, 0, header.Length);

        bool isJpeg = header[0] == 0xFF && header[1] == 0xD8;
        bool isPng = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E;

        if (!isJpeg && !isPng)
            return BadRequest("Invalid file type. Only JPEG and PNG allowed.");

        // Virus scan
        if (!await _virusScan.ScanAsync(file))
            return BadRequest("File contains malware or is suspicious.");

        // In a real app, encrypt and store. Here we just return success.
        return Ok(new { message = "File uploaded and passed all checks." });
    }
}