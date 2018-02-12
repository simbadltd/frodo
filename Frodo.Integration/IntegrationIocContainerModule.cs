using System.Collections.Generic;
using System.Reflection;
using Frodo.Core;
using Frodo.Infrastructure.Ioc;
using Frodo.Integration.Jira;
using Frodo.Integration.Toggl;

namespace Frodo.Integration
{
    internal class IntegrationIocContainerModule : IocContainerConfigurationModule
    {
        public IntegrationIocContainerModule(List<Assembly> assembliesToScan) : base(assembliesToScan)
        {
        }

        public override ICollection<TypeRegistration> TypeRegistrations()
        {
            var result = new List<TypeRegistration>
            {
                TypeRegistration.For<TogglTimeEntriesImportService, ITimeEntriesImportService>(Lifetime.PerLifetimeScope),
                TypeRegistration.For<JiraTimeEntriesExportService, ITimeEntriesExportService>(Lifetime.PerLifetimeScope),
            };

            return result;
        }
    }
}
