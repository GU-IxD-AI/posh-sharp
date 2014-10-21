using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarGP.operators
{
    public interface IFitnessEvaluator
    {
        bool IsFitter(object one, object two);


    }
}
