using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SupportChat.Backend.Services.Interfaces;

namespace SupportChat.Backend.Services;

public class EmailSettings
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword),
                EnableSsl = _settings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent to {To} with subject {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }

    public async Task SendEmailWithTemplateAsync(string to, string templateKey, Dictionary<string, string> placeholders)
    {
        // In production, fetch template from the database
        // Replace placeholders and send
        // For now, we'll send a simple default
        var subject = $"Support Chat: {templateKey}";
        var body = $"Hello,<br/><br/>This is a notification from Support Chat.<br/><br/>";

        // Replace placeholders
        foreach (var placeholder in placeholders)
        {
            body = body.Replace($"{{{placeholder.Key}}}", placeholder.Value);
        }

        await SendEmailAsync(to, subject, body);
    }
}