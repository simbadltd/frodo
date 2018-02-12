using System.Linq;
using System.Net.Mail;
using FluentEmail.Smtp;
using Frodo.Infrastructure.Configuration;

namespace Frodo.Infrastructure.Email
{
    public sealed class EmailSendService : IEmailSendService
    {
        private readonly SmtpSender _smtpSender;

        private readonly IEmailSettings _settings;

        public EmailSendService(ISettingsProvider settingsProvider)
        {
            _settings = settingsProvider.Settings.EmailSettings;

            _smtpSender = new SmtpSender(new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl,
            })
            {
                UseSsl = false,
            };
        }

        public void Send(string to, string subject, string body)
        {
            var email = FluentEmail.Core.Email.From(_settings.FromMailAddress).Subject(subject).To(to).Body(body);
            email.Sender = _smtpSender;
            email.Send();
        }
    }
}