using AssemblyUnloadableSignalR.AssemblyLoading;
using Contract;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AssemblyUnloadableSignalR
{
    public class Worker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ExecuteRunner();
        }

        public async Task ExecuteRunner()
        {
#warning Replace with path to your Runner.dll
            string path = @"C:\\Users\\benjamin\\source\\repos\\AssemblyUnloadableSignalR\\Runner\\bin\\Debug\\net5.0\\Runner.dll";

            await Module.Run<IAgentRunner>(path,
                async (module) =>
                {
                    try
                    {
                        await module.Initialize();
                        await module.StartRunner();
                        // waits 30 seconds to allow signalR connection to start before stopping
                        Thread.Sleep(30000);
                        await module.StopRunner();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error while executing Runner");
                    }
                });
        }
    }
}
