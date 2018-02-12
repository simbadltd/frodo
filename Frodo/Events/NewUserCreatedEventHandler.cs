using System;
using System.Text;
using Frodo.Core.Events;
using Frodo.Core.Repositories;
using Frodo.Infrastructure.Configuration;
using Frodo.Infrastructure.Email;

namespace Frodo.Events
{
    public sealed class NewUserCreatedEventHandler : IEventHandler<NewUserCreatedEvent>
    {
        private readonly IEmailSendService _emailSendService;

        private readonly ISettings _settings;

        public NewUserCreatedEventHandler(IEmailSendService emailSendService, ISettingsProvider settingsProvider)
        {
            _emailSendService = emailSendService;
            _settings = settingsProvider.Settings;
        }

        public void Handle(NewUserCreatedEvent domainEvent)
        {
            var user = domainEvent.User;
            var body = new StringBuilder();
            body.AppendLine($"Hello {user.Login},");
            body.AppendLine();
            body.AppendLine($"To activate your account, you must follow this link:");
            body.AppendLine(
                $"http://{_settings.Domain}:{_settings.Port}/Register/{user.Id:N}/activate/{user.ActivationKey}");

            _emailSendService.Send(user.Email, "[Frodo] Activation link", body.ToString());
        }
    }
}