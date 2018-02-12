using System.Collections.Generic;
using Autofac;
using Frodo.Infrastructure.Ioc;

namespace Frodo.Composition
{
    public static class IocContainer
    {
        public static IIocContainer Compose(params TypeRegistration[] typeRegistrations)
        {
            return new AutofacIocContainerAdapter(typeRegistrations);
        }

        public static IIocContainer Compose(ICollection<TypeRegistration> typeRegistrations = null, ICollection<InstanceRegistration> instanceRegistrations = null)
        {
            return new AutofacIocContainerAdapter(typeRegistrations, instanceRegistrations);
        }
        
        public static IIocContainer Compose(ILifetimeScope lifetimeScope)
        {
            return new AutofacIocContainerAdapter(lifetimeScope);
        }
    }
}