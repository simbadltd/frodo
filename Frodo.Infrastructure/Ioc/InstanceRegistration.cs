using System;

namespace Frodo.Infrastructure.Ioc
{
    public sealed class InstanceRegistration
    {
        public InstanceRegistration(object implementation, Type registrationType, Lifetime lifetime)
        {
            RegistrationType = registrationType;
            Implementation = implementation;
            Lifetime = lifetime;
        }

        public Type RegistrationType { get; private set; }

        public object Implementation { get; private set; }

        public Lifetime Lifetime { get; private set; }

        public static InstanceRegistration For<TRegistration>(object instance, Lifetime lifetime)
        {
            return new InstanceRegistration(instance, typeof(TRegistration), lifetime);
        }
    }
}