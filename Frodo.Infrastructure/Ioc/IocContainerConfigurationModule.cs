using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Frodo.Common;

namespace Frodo.Infrastructure.Ioc
{
    public abstract class IocContainerConfigurationModule : IIocContainerConfigurationModule
    {
        protected IocContainerConfigurationModule(List<Assembly> assembliesToScan)
        {
            AssembliesToScan = assembliesToScan.ToArray();
        }

        private const string ServiceWildCard = "*Service";

        private const string FactoryWildCard = "*Factory";

        protected Assembly[] AssembliesToScan { get; }

        protected static bool IsService(Type x)
        {
            return StringUtils.MatchWildcard(ServiceWildCard, x.Name);
        }

        protected static bool IsFactory(Type x)
        {
            return StringUtils.MatchWildcard(FactoryWildCard, x.Name);
        }

        protected ICollection<Type> AllTypes(Func<Type, bool> predicate)
        {
            return AssembliesToScan.SelectMany(GetLoadableTypes).Where(predicate).ToList();
        }

        protected ICollection<Type> AllTypesDerivedFrom<T>()
        {
            return AllTypesDerivedFrom(typeof(T));
        }

        protected ICollection<Type> AllTypesDerivedFrom(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            var result = AllTypes(x => x.GetTypeInfo().IsDerivedFrom(typeInfo)).ToList();
            return result;
        }

        public virtual ICollection<TypeRegistration> TypeRegistrations()
        {
            return Enumerable.Empty<TypeRegistration>().ToArray();
        }

        public virtual ICollection<InstanceRegistration> InstanceRegistrations()
        {
            return Enumerable.Empty<InstanceRegistration>().ToArray();
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            try
            {
                return assembly.DefinedTypes.Select(t => t.AsType()).Where(t => t.IsClass && t.IsAbstract == false);
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
        }
    }
}
