using System.Collections.Generic;
using System.Reflection;
using FluentValidation;
using Frodo.Core.Events;
using Frodo.Events;
using Frodo.Infrastructure.Ioc;
using Frodo.WebApp;
using Frodo.WebApp.Authentication;
using Nancy;
using Nancy.Authentication.Forms;

namespace Frodo
{
    internal class AppIocContainerModule : IocContainerConfigurationModule
    {
        public AppIocContainerModule(List<Assembly> assembliesToScan) : base(assembliesToScan)
        {
        }

        public override ICollection<TypeRegistration> TypeRegistrations()
        {
            var result = new List<TypeRegistration>
            {
                TypeRegistration.For<ImportFeature, IImportFeature>(Lifetime.PerLifetimeScope),
                TypeRegistration.For<ExportFeature, IExportFeature>(Lifetime.PerLifetimeScope),
                TypeRegistration.For<MembershipFeature, IMembershipFeature>(Lifetime.PerLifetimeScope),
                TypeRegistration.For<ActivateUserFeature, IActivateUserFeature>(Lifetime.PerLifetimeScope),
                
                TypeRegistration.For<PasswordCheckService, IPasswordCheckService>(Lifetime.PerLifetimeScope),
                TypeRegistration.For<UserMapper, IUserMapper>(Lifetime.PerLifetimeScope),
                TypeRegistration.For<DomainEventDispatcher, IDomainEventDispatcher>(Lifetime.PerLifetimeScope),
                
                TypeRegistration.For<AuthenticationService, IAuthenticationService>(Lifetime.Singleton),
            };
            
            result.AddRange(TypeRegistration.AsSelf(AllTypesDerivedFrom<NancyModule>(), Lifetime.Transient));
            result.AddRange(TypeRegistration.AsSelf(AllTypesDerivedFrom<IValidator>(), Lifetime.Transient));

            return result;
        }
    }
}
