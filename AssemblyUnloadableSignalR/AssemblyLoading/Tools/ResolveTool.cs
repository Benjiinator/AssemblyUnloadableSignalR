using AssemblyUnloadableSignalR.AssemblyLoading.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AssemblyUnloadableSignalR.AssemblyLoading.Tools
{
    public static class ResolveTool
    {
        public static T ResolveInstance<T>(Assembly assembly) where T : class
        {
            var types = GetTypesWithInterface<T>(assembly);
            var type = types.First();
            return Activator.CreateInstance(type) as T;
        }

        public static IEnumerable<Type> GetTypesWithInterface<T>(Assembly assembly) where T : class
        {
            var it = typeof(T);
            return assembly.GetLoadableTypes().Where(it.IsAssignableFrom).ToList();
        }
    }
}
