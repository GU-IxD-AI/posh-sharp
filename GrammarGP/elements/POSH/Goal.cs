using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;

namespace GrammarGP.elements.POSH
{
    class Goal : AGene
    {
        public Goal(Configuration config) 
            : base (config, GeneType.Goal, ReturnType.Bool)

        {
            childenReturnType = ReturnType.Bool;
        }

        public Goal(IChromosome chrom, Configuration config)
            : this(config)
        {
            SetChromosome(chrom);
        }

        public override string ToString()
        {
            return string.Empty;
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            string plan;
            elements = (elements is Dictionary<string, string>) ? elements : new Dictionary<string, string>();

            // taking apart the senses and putting them into the right form
            plan = "( ";

            foreach(decimal senseID in children)
                plan += m_Chromosome.GetGene(senseID).ToSerialize(elements) + " ";
            
            plan += ")";

            return plan;
        }

        public override bool SetChildren(AGene[] children)
        {
            for (int i = 0; i < children.Length; i++)
                if (children[i] is POSHSense)
                    childTypes.Add(GeneType.Sense);
            
            return base.SetChildren(children);
        }

        public override object Clone()
        {
            return new Goal(gpConfig);
        }
    }
}
