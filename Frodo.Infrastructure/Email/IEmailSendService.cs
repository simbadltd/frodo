namespace Frodo.Infrastructure.Email
{
    public interface IEmailSendService
    {
        void Send(string to, string subject, string body);
    }
}