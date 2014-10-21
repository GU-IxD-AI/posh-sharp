using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarGP.elements
{
    public interface IProgramPool
    {

        bool AddProgram(IProgram prog);

        IProgram GetProgram(string agentID);

        bool RemoveProgram(string agentID);

        bool RemoveProgram(IProgram prog);

    }
}
