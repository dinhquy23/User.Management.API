using User.Management.Service.Models;

namespace User.Management.Service.Services
{
    public interface IEmailServiceCustom
    {
        void SendEmail(Message message);
    }
}
