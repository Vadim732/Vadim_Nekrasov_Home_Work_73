using MailKit.Net.Smtp;
using MimeKit;

namespace MyChat.Services;

public class EmailService
{
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var emailMessage = new MimeMessage();
        
        emailMessage.From.Add(new MailboxAddress("Кто-то", "uskjgsjhd@mail.ru"));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text)
        {
            Text = message
        };
        
        using (var client = new SmtpClient())
        {
            await client.ConnectAsync("smtp.mail.ru", 465, true);
            await client.AuthenticateAsync("uskjgsjhd@mail.ru", "CXapDmMDHDs5m9nFeqpr");
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}