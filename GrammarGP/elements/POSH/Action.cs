using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;
using POSH.sys;

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

        public override AGene Mutate(float mutation)
        {
            // actions are represented as basic strings so mutating them is not possible at this level 
            // if actions should be changed xover is needed
            List<decimal> existing = new List<decimal>();
            AGene[] pool = new AGene[0];
            
            if (mutation < 0.75f)
            {
                existing.AddRange(m_Chromosome.GetAllInterChangeableGenes(type, returnType, false));
                existing.AddRange(m_Chromosome.GetAllInterChangeableGenes(type, returnType, true));

            }
            else
            {   pool = gpConfig.genePool.GetAllGenes(GeneType.Action);
                for (int i = 0; i < pool.Length; i++)
                {
                    existing.Add(-1-i);
                }
            }
            int pick = (int) MutateNumber(mutation,new Tuple<double,double>(0,existing.Count));
            
            return (existing[pick] < 0) ? (AGene) pool[(int)(Math.Abs(existing[pick]) - 1)].Clone() : (AGene) m_Chromosome.GetGene(existing[pick]).Clone();
        }

        public override bool SetChildren(AGene[] children)
        {
            return false;
        }
    }
}
