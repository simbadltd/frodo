using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core.Lifetime;
using AutoMapper;
using Frodo.Common;
using Frodo.Infrastructure.Ioc;

namespace Frodo.Composition
{
    internal sealed class AutofacIocContainerAdapter : IIocContainer
    {
        private static readonly string[] AssembliesWildCards = { "*Frodo.*dll", "*Frodo.exe" };

        private readonly ILifetimeScope _autofacContainer;

        private bool _disposed = false;

        public AutofacIocContainerAdapter(ICollection<TypeRegistration> typeRegistrations = null, ICollection<InstanceRegistration> instanceRegistrations = null)
        {
            var builder = new ContainerBuilder();

            var assembliesToScan = GetAssemblies(PathUtils.GetApplicationRoot(), AssembliesWildCards);
            RegisterSettings(builder);

            var modules = IocContainerConfigurationModuleProvider.GetAllModules(assembliesToScan);
            foreach (var module in modules)
            {
                RegisterModule(builder, module);
            }

            RegisterAutoMapper(builder, assembliesToScan);

            RegisterTypesInternal(builder, typeRegistrations);
            RegisterInstancesInternal(builder, instanceRegistrations);

            builder.RegisterInstance(this).As<IIocContainer>();

            _autofacContainer = builder.Build();
        }

        private void RegisterModule(ContainerBuilder builder, IocContainerConfigurationModule module)
        {
            RegisterTypesInternal(builder, module.TypeRegistrations());
            RegisterInstancesInternal(builder, module.InstanceRegistrations());
        }

        private void RegisterInstancesInternal(ContainerBuilder builder, ICollection<InstanceRegistration> instanceRegistrations)
        {
            if (instanceRegistrations == null)
            {
                return;
            }

            foreach (var instanceRegistration in instanceRegistrations)
            {
                RegisterInstanceInternal(builder, instanceRegistration);
            }
        }

        private void RegisterAutoMapper(ContainerBuilder builder, Assembly[] assembliesToScan)
        {
            builder.RegisterAssemblyTypes(assembliesToScan).AssignableTo<Profile>().As<Profile>();
            builder.RegisterAssemblyTypes(assembliesToScan).AsClosedTypesOf(typeof(ITypeConverter<,>)).AsSelf();

            builder.Register(
                    c => new MapperConfiguration(
                        cfg =>
                        {
                            var profiles = c.Resolve<IEnumerable<Profile>>();
                            foreach (var profile in profiles)
                            {
                                cfg.AddProfile(profile);
                            }
                        }))
                .AsSelf()
                .SingleInstance();

            builder.Register(c => c.Resolve<MapperConfiguration>().CreateMapper(Resolve))
                .As<IMapper>()
                .InstancePerLifetimeScope();
        }

        private void RegisterSettings(ContainerBuilder builder)
        {
            //builder.RegisterType<SettingsProvider>().As<ISettingsProvider>().SingleInstance();
            //builder.Register(context => context.Resolve<ISettingsProvider>().Settings).As<ISettings>();
        }

        public AutofacIocContainerAdapter(ILifetimeScope lifetimeScope)
        {
            _autofacContainer = lifetimeScope;
        }

        public static Assembly[] GetAssemblies(string path, params string[] assembliesWildCards)
        {
            return Directory.GetFiles(path).Where(f => assembliesWildCards.Any(x => StringUtils.MatchWildcard(x, Path.GetFileName(f)))).Select(Assembly.LoadFrom).ToArray();
        }

        public T Resolve<T>()
        {
            return _autofacContainer.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return _autofacContainer.Resolve(type);
        }

        public IIocContainer BeginLifetimeScope(
            ICollection<TypeRegistration> typeRegistrations = null,
            ICollection<InstanceRegistration> instanceRegistrations = null)
        {
            return IocContainer.Compose(
                _autofacContainer.BeginLifetimeScope(builder =>
                {
                    RegisterTypesInternal(builder, typeRegistrations);
                    RegisterInstancesInternal(builder, instanceRegistrations);
                }));
        }

        public IIocContainer BeginLifetimeScopeForRequest(ICollection<TypeRegistration> typeRegistrations = null, ICollection<InstanceRegistration> instanceRegistrations = null)
        {
            return BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag, typeRegistrations, instanceRegistrations);
        }

        public IIocContainer BeginLifetimeScope(
            object tag,
            ICollection<TypeRegistration> typeRegistrations = null,
            ICollection<InstanceRegistration> instanceRegistrations = null)
        {
            return IocContainer.Compose(
                _autofacContainer.BeginLifetimeScope(
                    tag,
                    builder =>
                    {
                        RegisterTypesInternal(builder, typeRegistrations);
                        RegisterInstancesInternal(builder, instanceRegistrations);
                    }));
        }

        private void RegisterTypesInternal(ContainerBuilder builder, ICollection<TypeRegistration> typeRegistrations)
        {
            if (typeRegistrations == null) return;

            foreach (var typeRegistration in typeRegistrations)
            {
                RegisterTypeInternal(builder, typeRegistration);
            }
        }

        public void RegisterInstance<TImplementation, TRegistration>(TImplementation obj, Lifetime lifetime) where TImplementation: class
        {
            var builder = new ContainerBuilder();
            RegisterInstanceInternal(builder, new InstanceRegistration(obj, typeof(TRegistration), lifetime));

            builder.Update(_autofacContainer.ComponentRegistry);
        }

        public void RegisterInstances(InstanceRegistration[] instanceRegistrations)
        {
            var builder = new ContainerBuilder();
            RegisterInstancesInternal(builder, instanceRegistrations);
            builder.Update(_autofacContainer.ComponentRegistry);
        }

        private void RegisterInstanceInternal(ContainerBuilder builder, InstanceRegistration instanceRegistration)
        {
            var registrationBuilder = builder.RegisterInstance(instanceRegistration.Implementation).As(instanceRegistration.RegistrationType);
            switch (instanceRegistration.Lifetime)
            {
                case Lifetime.Transient:
                    registrationBuilder.InstancePerDependency();
                    break;

                case Lifetime.Singleton:
                    registrationBuilder.SingleInstance();
                    break;

                case Lifetime.PerLifetimeScope:
                    registrationBuilder.InstancePerLifetimeScope();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(instanceRegistration.Lifetime));
            }
        }

        public void UpdateRegistration(TypeRegistration[] typeRegistrations)
        {
            UpdateRegistrationInternal(typeRegistrations);
        }

        public void UpdateRegistration<TImplementation, TRegistration>(Lifetime lifetime)
        {
            UpdateRegistrationInternal(new[] { new TypeRegistration(typeof(TImplementation), typeof(TRegistration), lifetime), });
        }

        public void UpdateRegistration(Type implementationType, Type registrationType, Lifetime lifetime)
        {
            UpdateRegistrationInternal(new[] { new TypeRegistration(implementationType, registrationType, lifetime), });
        }

        public void UpdateRegistrationInternal(TypeRegistration[] typeRegistrations)
        {
            var builder = new ContainerBuilder();

            foreach (var typeRegistration in typeRegistrations)
            {
                RegisterTypeInternal(builder, typeRegistration);
            }

            builder.Update(_autofacContainer.ComponentRegistry);
        }

        private void RegisterTypeInternal(ContainerBuilder builder, TypeRegistration typeRegistration)
        {
            if (typeRegistration.IsOpenGenericRegistration)
            {
                var registration = builder.RegisterGeneric(typeRegistration.ImplementationType).As(typeRegistration.RegistrationType);
                ApplyLifetime(registration, typeRegistration.Lifetime);
            }
            else
            {
                var registration = builder.RegisterType(typeRegistration.ImplementationType).As(typeRegistration.RegistrationType);
                ApplyLifetime(registration, typeRegistration.Lifetime);
            }
        }

        private void ApplyLifetime<TLimit, TActivatorData, TRegistrationStyle>(
            IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registrationBuilder,
            Lifetime lifetime)
        {
            switch (lifetime)
            {
                case Lifetime.Transient:
                    {
                        registrationBuilder.InstancePerDependency();
                        break;
                    }

                case Lifetime.Singleton:
                    {
                        registrationBuilder.SingleInstance();
                        break;
                    }

                case Lifetime.PerLifetimeScope:
                    {
                        registrationBuilder.InstancePerLifetimeScope();
                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime));
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _autofacContainer.Dispose();
                }

                _disposed = true;
            }
        }

        ~AutofacIocContainerAdapter()
        {
            Dispose(false);
        }
    }
}