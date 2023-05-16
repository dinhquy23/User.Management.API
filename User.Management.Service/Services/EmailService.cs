using MailKit.Net.Smtp;
using MimeKit;
using User.Management.Service.Models;

namespace User.Management.Service.Services
{
    public class EmailService : IEmailServiceCustom
    {
        private readonly EmailConfiguration _configuration;
        public EmailService(EmailConfiguration emailConfiguration)
        {
            _configuration = emailConfiguration;
        }
        public void SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);
            Send(emailMessage);
        }

        public void Send(MimeMessage emailMessage)
        {
            using var client = new SmtpClient();
            try
            {
                client.Connect(_configuration.SmtpServer, _configuration.Port, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(_configuration.UserName, _configuration.Password);
                client.Send(emailMessage);
            }
            catch
            {
                throw;
            }
            finally 
            {
                client.Disconnect(true);
                client.Dispose();
            }
        }

        public MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("dinhquy", _configuration.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = message.Content };
            return emailMessage;
        }
    }
}
