using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;

namespace GrammarGP.elements
{
    public interface IProgram
    {
        IChromosome GetChromosome();

        AgentBase GetAgent();

        double GetFitnessValue();

    }
}
