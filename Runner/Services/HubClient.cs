using Microsoft.AspNetCore.SignalR.Client;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Runner.Services
{
    public class HubClient
    {
        public HubConnection HubConnection;
        protected async Task BuildHubAsync(string url)
        {
            HubConnection = new HubConnectionBuilder().WithUrl(url).Build();
        }

        public virtual async Task Start()
        {
            if (HubConnection.State == HubConnectionState.Disconnected)
                await HubConnection.StartAsync();
            System.Console.WriteLine($"HubState = {HubConnection.State}");
        }

        public virtual async Task StopAsync()
        {
            await HubConnection.StopAsync();
            await HubConnection.DisposeAsync();
        }
    }
}
