using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.elements;

namespace GrammarGP.operators
{
    public interface IFitnessEvaluator
    {
        bool IsFitter(object one, object two);

		int Compare (IProgram program1, IProgram program2);

    }
}
