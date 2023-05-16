using MimeKit;

namespace User.Management.Service.Models
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string? Subject { get; set; }
        public string? Content { get; set; }
        public Message(IEnumerable<string> mailboxes, string subject, string content)
        {
            To = new List<MailboxAddress>();
            To.AddRange(mailboxes.Select(item => new MailboxAddress("",item)));
            Subject = subject;
            Content = content;
        }
    }
}
