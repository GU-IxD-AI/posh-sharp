using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.elements;
using GrammarGP.env;

namespace GrammarGP.operators
{
    public interface ISelectOperator
    {
        void AddPrograms(IProgram[] progs);

        void AddProgram(IProgram progs);

        void SetConfiguration(Configuration config);

        IProgram SelectIndividuum(GenoType genotype);

        bool Reset();

        void Empty();

    }
}
