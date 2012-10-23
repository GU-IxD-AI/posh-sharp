using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys.scheduled
{
    public class ScheduledAgent : AgentBase
    {
        public ScheduledAgent(string library, string plan, Dictionary<Tuple<string,string>,object> attributes, World world = null)
            : base(library,plan,attributes,world)
        {
        }
    }
}
