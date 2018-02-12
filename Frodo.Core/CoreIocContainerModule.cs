using System.Collections.Generic;
using System.Reflection;
using Frodo.Core.Events;
using Frodo.Infrastructure.Ioc;

namespace Frodo.Core
{
    internal class CoreIocContainerModule : IocContainerConfigurationModule
    {
        public CoreIocContainerModule(List<Assembly> assembliesToScan) : base(assembliesToScan)
        {
        }

        public override ICollection<TypeRegistration> TypeRegistrations()
        {
            var result = new List<TypeRegistration>
            {
                TypeRegistration.For<DefaultTaskCommentParsingLogic, ITaskCommentParsingLogic>(
                    Lifetime.PerLifetimeScope),
            };

            result.AddRange(TypeRegistration.AsImplementedInterfaces(AllTypesDerivedFrom(typeof(IEventHandler<>)),
                Lifetime.PerLifetimeScope));

            return result;
        }
    }
}