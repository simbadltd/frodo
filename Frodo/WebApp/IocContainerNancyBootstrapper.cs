using System;
using System.Collections.Generic;
using System.Linq;
using Frodo.Composition;
using Frodo.Infrastructure.Ioc;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Diagnostics;
using InstanceRegistration = Nancy.Bootstrapper.InstanceRegistration;
using Lifetime = Frodo.Infrastructure.Ioc.Lifetime;
using TypeRegistration = Nancy.Bootstrapper.TypeRegistration;

namespace Frodo.WebApp
{
    public abstract class IocContainerNancyBootstrapper : NancyBootstrapperWithRequestContainerBase<IIocContainer>
    {
        protected override IDiagnostics GetDiagnostics()
        {
            return this.ApplicationContainer.Resolve<IDiagnostics>();
        }

        protected override IEnumerable<IApplicationStartup> GetApplicationStartupTasks()
        {
            return this.ApplicationContainer.Resolve<IEnumerable<IApplicationStartup>>();
        }

        protected override IEnumerable<IRequestStartup> RegisterAndGetRequestStartupTasks(IIocContainer container, Type[] requestStartupTypes)
        {
            var typeRegistrations =
                requestStartupTypes.Select(x => new Infrastructure.Ioc.TypeRegistration(x, typeof(IRequestStartup), Lifetime.Transient)).ToArray();

            container.UpdateRegistration(typeRegistrations);
            return container.Resolve<IEnumerable<IRequestStartup>>();
        }

        protected override IEnumerable<IRegistrations> GetRegistrationTasks()
        {
            return this.ApplicationContainer.Resolve<IEnumerable<IRegistrations>>();
        }

        protected override INancyEngine GetEngineInternal()
        {
            return this.ApplicationContainer.Resolve<INancyEngine>();
        }

        protected override IIocContainer GetApplicationContainer()
        {
            return IocContainer.Compose();
        }

        protected override void RegisterBootstrapperTypes(IIocContainer applicationContainer)
        {
            applicationContainer.RegisterInstance<IocContainerNancyBootstrapper, INancyModuleCatalog>(this, Lifetime.Singleton);
        }

        protected override void RegisterTypes(IIocContainer container, IEnumerable<TypeRegistration> typeRegistrations)
        {
            var convertedTypeRegistrations = typeRegistrations.Select(ConvertNancyTypeRegistration).ToArray();

            container.UpdateRegistration(convertedTypeRegistrations);
        }

        protected override void RegisterCollectionTypes(IIocContainer container, IEnumerable<CollectionTypeRegistration> collectionTypeRegistrations)
        {
            var typeRegistrations =
                collectionTypeRegistrations.SelectMany(
                        t =>
                            t.ImplementationTypes.Select(
                                x => new Infrastructure.Ioc.TypeRegistration(x, t.RegistrationType, ConvertNancyLifetime(t.Lifetime))))
                    .ToArray();

            container.UpdateRegistration(typeRegistrations);
        }

        protected override void RegisterInstances(IIocContainer container, IEnumerable<InstanceRegistration> instanceRegistrations)
        {
            var convertedInstanceRegistrations = instanceRegistrations.Select(ConvertNancyInstanceRegistration).ToArray();
            container.RegisterInstances(convertedInstanceRegistrations);
        }

        protected override IIocContainer CreateRequestContainer(NancyContext context)
        {
            return this.ApplicationContainer.BeginLifetimeScopeForRequest();
        }

        protected override void RegisterRequestContainerModules(IIocContainer container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
        {
            var convertedModuleRegistrations = moduleRegistrationTypes.Select(ConvertNancyModuleRegistration).ToArray();
            container.UpdateRegistration(convertedModuleRegistrations);
        }

        protected override IEnumerable<INancyModule> GetAllModules(IIocContainer container)
        {
            return container.Resolve<IEnumerable<INancyModule>>();
        }

        protected override INancyModule GetModule(IIocContainer container, Type moduleType)
        {
            container.UpdateRegistration(moduleType, typeof(INancyModule), Lifetime.Transient);
            return container.Resolve(moduleType) as INancyModule;
        }

        private static Infrastructure.Ioc.TypeRegistration ConvertNancyModuleRegistration(ModuleRegistration typeRegistration)
        {
            return new Infrastructure.Ioc.TypeRegistration(typeRegistration.ModuleType, typeof(INancyModule), Lifetime.Transient);
        }

        private Infrastructure.Ioc.InstanceRegistration ConvertNancyInstanceRegistration(InstanceRegistration instanceRegistration)
        {
            return new Infrastructure.Ioc.InstanceRegistration(
                instanceRegistration.Implementation,
                instanceRegistration.RegistrationType,
                ConvertNancyLifetime(instanceRegistration.Lifetime));
        }

        private Infrastructure.Ioc.TypeRegistration ConvertNancyTypeRegistration(TypeRegistration typeRegistration)
        {
            return new Infrastructure.Ioc.TypeRegistration(
                typeRegistration.ImplementationType,
                typeRegistration.RegistrationType,
                ConvertNancyLifetime(typeRegistration.Lifetime));
        }

        private Lifetime ConvertNancyLifetime(Nancy.Bootstrapper.Lifetime lifetime)
        {
            switch (lifetime)
            {
                case Nancy.Bootstrapper.Lifetime.PerRequest:
                    return Lifetime.PerLifetimeScope;

                case Nancy.Bootstrapper.Lifetime.Singleton:
                    return Lifetime.Singleton;

                case Nancy.Bootstrapper.Lifetime.Transient:
                    return Lifetime.Transient;

                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime));
            }
        }
    }
}