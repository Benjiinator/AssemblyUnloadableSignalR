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
            // We're not awaiting the ExecuteAndUnload because that would keep anything rooted by the stack
            // alive. This includes thing rooted by the child ALC we just created and the ALC itself when the 
            // continuation is being executed.
            // RunContinuationsAsynchronously is important to resume this logic in a different stack
            var tcs = new TaskCompletionSource<WeakReference>(TaskCreationOptions.RunContinuationsAsynchronously);

            _ = ExecuteAndUnload<T>(executeModule, executeModule, action, tcs);

            WeakReference hostAlcWeakRef = await tcs.Task;

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
        static async Task ExecuteAndUnload<T>(string modulePath, string resolvePath, Func<T, Task> action, TaskCompletionSource<WeakReference> tcs) where T : class
        {
            try
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

                tcs.TrySetResult(alcWeakRef);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        }
    }
}
