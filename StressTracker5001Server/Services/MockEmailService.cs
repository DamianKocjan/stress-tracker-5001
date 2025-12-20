namespace StressTracker5001Server.Services
{
  public class MockEmailService : IEmailService
  {
    private readonly ILogger<MockEmailService> _logger;
    private readonly IConfiguration _configuration;

    public MockEmailService(ILogger<MockEmailService> logger, IConfiguration configuration)
    {
      _logger = logger;
      _configuration = configuration;
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken, string resetLink)
    {
      var logMessage = $@"
=== PASSWORD RESET EMAIL (MOCK) ===
To: {email}
Token: {resetToken}
Reset Link: {resetLink}
Sent At: {DateTime.UtcNow:o}
================================
";
      _logger.LogInformation("Mock Email Service: Sending password reset email to {Email}", email);
      _logger.LogInformation(logMessage);

      await LogToFileAsync("password_reset", email, resetToken, resetLink);
    }

    public async Task SendEmailVerificationAsync(string email, string verificationToken, string verificationLink)
    {
      var logMessage = $@"
=== EMAIL VERIFICATION EMAIL (MOCK) ===
To: {email}
Token: {verificationToken}
Verification Link: {verificationLink}
Sent At: {DateTime.UtcNow:o}
======================================
";
      _logger.LogInformation("Mock Email Service: Sending email verification to {Email}", email);
      _logger.LogInformation(logMessage);

      await LogToFileAsync("email_verification", email, verificationToken, verificationLink);
    }

    public async Task SendAccountDeletionNotificationAsync(string email, string userName)
    {
      var logMessage = $@"
=== ACCOUNT DELETION NOTIFICATION (MOCK) ===
To: {email}
User: {userName}
Deleted At: {DateTime.UtcNow:o}
=========================================
";
      _logger.LogInformation("Mock Email Service: Sending account deletion notification to {Email}", email);
      _logger.LogInformation(logMessage);

      await LogToFileAsync("account_deletion", email, userName, null);
    }

    private async Task LogToFileAsync(string emailType, string email, string? token, string? link)
    {
      try
      {
        var logsDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
        if (!Directory.Exists(logsDirectory))
        {
          Directory.CreateDirectory(logsDirectory);
        }

        var logFilePath = Path.Combine(logsDirectory, "email-sent.log");
        var logEntry = $"[{DateTime.UtcNow:o}] Type: {emailType} | Email: {email} | Token: {token ?? "N/A"} | Link: {link ?? "N/A"}\n";

        await System.IO.File.AppendAllTextAsync(logFilePath, logEntry);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error writing to email log file");
      }
    }
  }
}
