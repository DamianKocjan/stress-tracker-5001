namespace StressTracker5001Server.Services
{
  public interface IEmailService
  {
    Task SendPasswordResetEmailAsync(string email, string resetToken, string resetLink);
    Task SendEmailVerificationAsync(string email, string verificationToken, string verificationLink);
    Task SendAccountDeletionNotificationAsync(string email, string userName);
  }
}
