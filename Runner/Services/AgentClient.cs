using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Runner.Services
{
    public class AgentClient : HubClient
    {
        public Timer _tmrConnect;

        public async Task BuildAgentClientAsync()
        {
            try
            {
#warning replace with your signalR url
                await BuildHubAsync("yoursignalrurl");
            }
            catch (Exception)
            {
                Console.WriteLine("Could not reach SignalR Hub");
            }
        }

        public override async Task Start()
        {
            try
            {
                if (HubConnection == null)
                    await BuildAgentClientAsync();

                await base.Start();
            }
            catch (Exception)
            {
                Console.WriteLine("Could not start SignalR Connection");
            }
        }

        public async void OnConnect(object state)
        {
            StopTimer(_tmrConnect);

            await Start();
            _tmrConnect.Change(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10));
        }

        public void StopTimer(Timer timer)
        {
            if (timer == null)
                return;

            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public override Task StopAsync()
        {
            // Stop the timer here since that ends up rooting the callback in the global timer queue
            StopTimer(_tmrConnect);
            return base.StopAsync();
        }
    }
}
