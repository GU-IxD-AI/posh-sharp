using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;
using POSH.sys;

namespace GrammarGP.elements.POSH
{
    public class POSHSense : AGene
    {
        public POSHSense(Configuration config, object value) 
            : base (config, GeneType.Sense, ReturnType.Bool,value)

        {
            if (value is string)
                this.value = value;
            childTypes = new List<GeneType>(new GeneType[] { GeneType.Terminal, GeneType.Predicate});
        }

         public POSHSense(IChromosome chrom, Configuration config, object value) 
             : this(config,value)
         {
             SetChromosome(chrom);
         }

        public override object Clone()
        {
            //@TODO: cloning a subtree withouty children I am not sure if that is wise because of growing the subtree somewhere else requires a sanity check.
            return new POSHSense(gpConfig,value);

        }

        public override string ToString()
        {
            return (value is string) ? (string)value : string.Empty;
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            string plan = string.Empty;
            elements = (elements is Dictionary<string, string>) ? elements : new Dictionary<string, string>();

            
            if (m_Chromosome.GetGene(children[0]) is Predicate && m_Chromosome.GetGene(children[1]) is Terminal)
            // taking appart the senses and putting them into the right form
                plan = String.Format("( {0} {1} {2} )", ToString(), m_Chromosome.GetGene(children[1]).ToSerialize(elements), m_Chromosome.GetGene(children[0]).ToSerialize(elements));

            return plan;
        }

        /// <summary>
        /// Sets the children of a sense. THe first child is supposed to be a predicate and the second a terminal matching the same type as the sense's value.
        /// </summary>
        /// <param name="children"></param>
        public override bool SetChildren(AGene[] children)
        {
            return base.SetChildren(children);
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
            }
            else
            {
                pool = gpConfig.genePool.GetAllGenes(type);
                for (int i = 0; i < pool.Length; i++)
                {
                    existing.Add(-1 - i);
                }
            }
            int pick = (int)MutateNumber(mutation, new Tuple<double, double>(0, existing.Count));

            return (existing[pick] < 0) ? (AGene)pool[(int)(Math.Abs(existing[pick]) - 1)].Clone() : (AGene)m_Chromosome.GetGene(existing[pick]).Clone();
        }

        
    }
}
