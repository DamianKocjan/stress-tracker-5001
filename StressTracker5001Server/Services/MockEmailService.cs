namespace StressTracker5001Server.Services
{
    public class MockEmailService : IEmailService
    {
        private readonly ILogger<MockEmailService> _logger;

        public MockEmailService(ILogger<MockEmailService> logger)
        {
            _logger = logger;
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
        }
    }
}
