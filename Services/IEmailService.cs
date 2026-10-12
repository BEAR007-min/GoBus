namespace GoBus.Services;

public interface IEmailService
{
    Task SendOtpEmailAsync(
        string toEmail,
        string otp);
}