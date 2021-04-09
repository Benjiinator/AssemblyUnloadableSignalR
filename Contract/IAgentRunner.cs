using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Contract
{
    public interface IAgentRunner
    {
        Task Initialize();
        Task StartRunner();
        Task StopRunner();
    }
}
