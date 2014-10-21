using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.elements;

namespace GrammarGP.operators
{
    public interface ICrossOverOperator
    {
        IChromosome [] DoCross(IChromosome a_chrom, IChromosome b_chrom);

    }
}
