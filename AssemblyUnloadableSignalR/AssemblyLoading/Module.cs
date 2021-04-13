using AssemblyUnloadableSignalR.AssemblyLoading.Tools;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AssemblyUnloadableSignalR.AssemblyLoading
{
    public static class Module
    {
        public static async Task Run<T>(string executeModule, Func<T, Task> action) where T : class
        {
            WeakReference hostAlcWeakRef = await ExecuteAndUnload<T>(executeModule, executeModule, action, tcs);

            for (int i = 0; hostAlcWeakRef.IsAlive && (i < 10); i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            if (hostAlcWeakRef.IsAlive)
                Console.WriteLine($"Unloading {executeModule} failed");
            else
                Console.WriteLine($"Unloading {executeModule} succeeded");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static async Task<WeakReference> ExecuteAndUnload<T>(string modulePath, string resolvePath, Func<T, Task> action) where T : class
        {
            var alc = new ModuleAssemblyLoadContext(resolvePath);

            var typeDescriptorAssemblyPath = typeof(TypeDescriptor).Assembly.Location;
            alc.LoadFromAssemblyPath(typeDescriptorAssemblyPath);
            var netstandardShimAssemblyPath = Path.Combine(Path.GetDirectoryName(typeDescriptorAssemblyPath), "netstandard.dll");
            alc.LoadFromAssemblyPath(netstandardShimAssemblyPath);

            var alcWeakRef = new WeakReference(alc);

            Assembly a = alc.LoadFromAssemblyPath(modulePath);

            var assemblyResolverTool = new AssemblyResolverTool
            {
                dependencyContext = DependencyContext.Load(a),

                assemblyResolver = new CompositeCompilationAssemblyResolver
                                    (new ICompilationAssemblyResolver[]
            {
                new AppBaseCompilationAssemblyResolver(Path.GetDirectoryName(modulePath)),
                new ReferenceAssemblyPathResolver(),
                new PackageCompilationAssemblyResolver()
            })
            };

            alc.Resolving += assemblyResolverTool.OnResolving;

            var module = ResolveTool.ResolveInstance<T>(a);

            await action(module);

            alc.Resolving -= assemblyResolverTool.OnResolving;
            alc.Unload();
            
            // Yield right before leaving this method so that the only thing we capture on the stack is the weak reference
            await Task.Yield();
            
            return alcWeakRef;
        }
    }
}
