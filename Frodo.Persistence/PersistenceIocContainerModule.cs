using System.Collections.Generic;
using System.Reflection;
using Frodo.Core.Repositories;
using Frodo.Infrastructure.Ioc;
using Frodo.Persistence.Mapping;

namespace Frodo.Persistence
{
    internal class PersistenceIocContainerModule : IocContainerConfigurationModule
    {
        public override ICollection<TypeRegistration> TypeRegistrations()
        {
            var result = new List<TypeRegistration>
            {
                // SQLite
                TypeRegistration.For<SqliteStorageAdapter, IStorageAdapter>(Lifetime.PerLifetimeScope),
                TypeRegistration.For<UnitOfWork, IUnitOfWork>(Lifetime.PerLifetimeScope),
                TypeRegistration.OpenGeneric(typeof(Repository<>), typeof(IRepository<>), Lifetime.PerLifetimeScope),
            };

            // Models
            result.AddRange(TypeRegistration.AsImplementedInterfaces(AllTypesDerivedFrom<Dao>(), Lifetime.Transient));

            return result;
        }

        public PersistenceIocContainerModule(List<Assembly> assembliesToScan) : base(assembliesToScan)
        {
        }
    }
}