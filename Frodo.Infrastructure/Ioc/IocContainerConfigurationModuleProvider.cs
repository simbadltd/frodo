using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Frodo.Infrastructure.Ioc
{
    public static class IocContainerConfigurationModuleProvider
    {
        public static List<IocContainerConfigurationModule> GetAllModules(Assembly[] assemblies)
        {
            var result = new List<IocContainerConfigurationModule>();

            foreach (var assembly in assemblies)
            {
                var suitableTypes = assembly.GetTypes()
                    .Where(t => t.GetInterfaces().Any(i => i.Name == typeof(IIocContainerConfigurationModule).Name))
                    .ToList();

                foreach (var type in suitableTypes)
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    var module = (IocContainerConfigurationModule)Activator.CreateInstance(type, assemblies.ToList());
                    result.Add(module);
                }
            }

            return result;
        }
    }
}