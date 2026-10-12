using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using GoBus.Models;

namespace GoBus.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(
        IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendOtpEmailAsync(
        string toEmail,
        string otp)
    {
        var email = new MimeMessage();

        email.From.Add(
            new MailboxAddress(
                _settings.FromName,
                _settings.FromEmail));

        email.To.Add(
            MailboxAddress.Parse(toEmail));

        email.Subject = "GoBus - Mã OTP khôi phục mật khẩu";

        var body = $"""
        <html>
        <body style="font-family: Arial, sans-serif; background:#f5f7fb; padding:30px;">

            <div style="
                max-width:500px;
                margin:auto;
                background:white;
                padding:30px;
                border-radius:15px;
                box-shadow:0 4px 15px rgba(0,0,0,0.08);
            ">

                <h2 style="text-align:center; color:#0d6efd;">
                    🚌 GoBus
                </h2>

                <h3>
                    Khôi phục mật khẩu
                </h3>

                <p>
                    Bạn vừa yêu cầu khôi phục mật khẩu tài khoản GoBus.
                </p>

                <p>
                    Mã OTP của bạn là:
                </p>

                <div style="
                    text-align:center;
                    font-size:32px;
                    font-weight:bold;
                    letter-spacing:8px;
                    background:#f0f4ff;
                    padding:15px;
                    border-radius:10px;
                    color:#0d6efd;
                ">
                    {otp}
                </div>

                <p style="margin-top:20px;">
                    Mã OTP có hiệu lực trong <strong>5 phút</strong>.
                </p>

                <p>
                    Nếu bạn không yêu cầu khôi phục mật khẩu,
                    hãy bỏ qua email này.
                </p>

                <hr>

                <p style="font-size:12px;color:#777;text-align:center;">
                    GoBus - Hệ thống đặt vé xe khách
                </p>

            </div>

        </body>
        </html>
        """;

        email.Body = new BodyBuilder
        {
            HtmlBody = body
        }.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _settings.Host,
            _settings.Port,
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            _settings.Username,
            _settings.Password);

        await smtp.SendAsync(email);

        await smtp.DisconnectAsync(true);
    }
}