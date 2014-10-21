using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;

namespace GrammarGP.elements.POSH
{
    public class POSHAction : AGene
    {
        public POSHAction(Configuration config, object value)
            : base(config, GeneType.Action, ReturnType.Bool) 
        {
            if (value is string)
                this.value = value;

        }

        public POSHAction(IChromosome chrom, Configuration config, object value)
            : this(config,value)
        {
            SetChromosome(chrom);
        }

        public override object Clone()
        {
            return new POSHAction(gpConfig, value);
        }

        public override string ToString()
        {
            return (value is string) ? (string) value : string.Empty;
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            return this.ToString();
        }

        public override void Mutate(float mutation)
        {
            // actions are represented as basic strings so mutating them is not possible at this level 
            // if actions should be changed xover is needed
        }
    }
}
