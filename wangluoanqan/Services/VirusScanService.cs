namespace wangluoanqan.Services;

public class VirusScanService
{
    // Simulated virus scan – always passes in demo
    public async Task<bool> ScanAsync(IFormFile file)
    {
        // In real life, integrate with ClamAV or similar
        await Task.Delay(50); // simulate scanning time
        // Randomly fail 5% for demonstration
        return new Random().Next(1, 100) <= 95;
    }
}