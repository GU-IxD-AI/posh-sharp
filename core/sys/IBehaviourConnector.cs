using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace POSH.sys
{
    public interface IBehaviourConnector
    {
        string GetBehaviourLibrary();

        Behaviour[] GetBehaviours(AgentBase agent);

        string GetPlanFileStream(string planName);

        string GetInitFileStream(string initFileName);

        bool Ready();

    }
}
