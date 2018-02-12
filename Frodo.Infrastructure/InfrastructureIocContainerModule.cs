using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Frodo.Infrastructure.Configuration;
using Frodo.Infrastructure.Email;
using Frodo.Infrastructure.Ioc;
using Frodo.Infrastructure.Json;
using Frodo.Infrastructure.Logging;

namespace Frodo.Infrastructure
{
    internal class InfrastructureIocContainerModule : IocContainerConfigurationModule
    {
        public InfrastructureIocContainerModule(List<Assembly> assembliesToScan) : base(assembliesToScan)
        {
        }

        public override ICollection<TypeRegistration> TypeRegistrations()
        {
            return new[]
            {
                TypeRegistration.For<NLogManager, ILogManager>(Lifetime.Singleton),
                TypeRegistration.For<JsonSerializer, IJsonSerializer>(Lifetime.Singleton),
                TypeRegistration.For<SettingsProvider, ISettingsProvider>(Lifetime.Singleton),
                TypeRegistration.For<EmailSendService, IEmailSendService>(Lifetime.PerLifetimeScope),
            };
        }

        public override ICollection<InstanceRegistration> InstanceRegistrations()
        {
            return Enumerable.Empty<InstanceRegistration>().ToArray();
        }
    }
}