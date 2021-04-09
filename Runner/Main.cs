using Contract;
using Runner.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Runner
{
    public class Main : IAgentRunner
    {
        private AgentClient agentClient;

        public async Task Initialize()
        {
            agentClient = new AgentClient();
        }
        public async Task StartRunner()
        {
            agentClient._tmrConnect = new Timer(agentClient.OnConnect, null, Timeout.Infinite, Timeout.Infinite);
            agentClient._tmrConnect.Change(TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(10));
            return;
        }

        public async Task StopRunner()
        {
            await agentClient.StopAsync();
        }
    }
}
