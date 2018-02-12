using System;
using System.Collections.Generic;

namespace Frodo.Infrastructure.Ioc
{
    public interface IIocContainer : IDisposable
    {
        T Resolve<T>();

        object Resolve(Type type);

        IIocContainer BeginLifetimeScope(
            ICollection<TypeRegistration> typeRegistrations = null,
            ICollection<InstanceRegistration> instanceRegistrations = null);

        IIocContainer BeginLifetimeScope(
            object tag,
            ICollection<TypeRegistration> typeRegistrations = null,
            ICollection<InstanceRegistration> instanceRegistrations = null);

        IIocContainer BeginLifetimeScopeForRequest(
            ICollection<TypeRegistration> typeRegistrations = null,
            ICollection<InstanceRegistration> instanceRegistrations = null);

        void UpdateRegistration(TypeRegistration[] typeRegistrations);

        void UpdateRegistration(Type implementationType, Type registrationType, Lifetime lifetime);

        void UpdateRegistration<TImplementation, TRegistration>(Lifetime lifetime);

        void RegisterInstance<TImplementation, TRegistration>(TImplementation obj, Lifetime lifetime) where TImplementation : class;

        void RegisterInstances(InstanceRegistration[] instanceRegistrations);
    }
}