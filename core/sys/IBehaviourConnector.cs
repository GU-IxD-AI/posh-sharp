using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys
{
    public interface IBehaviourConnector
    {
        Behaviour[] GetBehaviours(AgentBase agent);

        void ConnectPOSHAgent(AgentBase agent);

        bool Ready();

    }
}
