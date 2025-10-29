using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using EventTicketingSystem.Services;

namespace EventTicketingSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();
            
            emailMessage.From.Add(new MailboxAddress(_configuration["EmailSettings:DisplayName"], _configuration["EmailSettings:Mail"]));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = message };
            
            // In a real application, you would send the email
            // For demo purposes, we'll just log it
            Console.WriteLine($"Email to: {email}, Subject: {subject}, Message: {message}");
            
            // Uncomment to actually send emails
            /*
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_configuration["EmailSettings:Host"], 
                    int.Parse(_configuration["EmailSettings:Port"]), false);
                await client.AuthenticateAsync(_configuration["EmailSettings:Mail"], 
                    _configuration["EmailSettings:Password"]);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
            */
        }
    }
}