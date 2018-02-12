using System.Collections.Generic;

namespace Frodo.Infrastructure.Ioc
{
    public interface IIocContainerConfigurationModule
    {
        ICollection<TypeRegistration> TypeRegistrations();

        ICollection<InstanceRegistration> InstanceRegistrations();
    }
}