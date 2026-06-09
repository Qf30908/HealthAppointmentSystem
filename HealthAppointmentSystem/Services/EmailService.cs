namespace HealthAppointmentSystem.Services;
using System.Net;
using System.Net.Mail;
using HealthAppointmentSystem.Models;
using Microsoft.Extensions.Options;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using var smtp = new SmtpClient("smtp.gmail.com")
        {
            Port = 587,
            Credentials = new NetworkCredential(
                _settings.Email,
                _settings.AppPassword),
            EnableSsl = true
        };

        var message = new MailMessage(
            _settings.Email,
            to,
            subject,
            body);

        await smtp.SendMailAsync(message);
    }
}
