namespace SupportChat.Backend.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    Task SendEmailWithTemplateAsync(string to, string templateKey, Dictionary<string, string> placeholders);
}