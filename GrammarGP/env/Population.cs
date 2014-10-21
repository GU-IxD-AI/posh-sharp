using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.elements;

namespace GrammarGP.env
{
    public class Population : IPopulation,ICloneable
    {
        public Configuration gpConfiguration { get; private set; }
        public IChromosomePool chromPool { get; private set; }
        public IProgramPool programmPool { get; private set; }


        private Population(Population oldPop)
            : this((Configuration) oldPop.gpConfiguration.Clone())
        {

        }

        public Population(Configuration conf)
        {
            this.gpConfiguration = conf;
        }

    
        public object  Clone()
        {
            return new Population(this);
        }
    }   
}
