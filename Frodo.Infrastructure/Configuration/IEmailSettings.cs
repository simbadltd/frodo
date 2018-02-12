namespace Frodo.Infrastructure.Configuration
{
    public interface IEmailSettings
    {
        string SmtpServer { get; set; }

        int SmtpPort { get; set; }

        bool EnableSsl { get; set; }

        string User { get; set; }

        string Password { get; set; }

        string FromMailAddress { get; set; }

        int Timeout { get; set; }
    }
}