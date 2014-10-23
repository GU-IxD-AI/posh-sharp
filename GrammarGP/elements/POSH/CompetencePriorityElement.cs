using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;

namespace GrammarGP.elements.POSH
{
    class CompetencePriorityElement :AGene
    {
        public CompetencePriorityElement(Configuration config,object value) 
            : base (config, GeneType.CompetencePriorityElement, ReturnType.Bool, value)

        {
            childenReturnType = ReturnType.Bool;
        }

        public CompetencePriorityElement(IChromosome chrom, Configuration config, object value)
            : this(config,value)
        {
            SetChromosome(chrom);
        }
        public override bool SetChildren(AGene[] children)
        {
            // children can only be CompetenceElements
            this.childTypes.Clear();

            foreach (CompetenceElement elem in children) 
                if (elem.type == GeneType.CompetenceElement)
                    childTypes.Add(GeneType.CompetenceElement);
                
            return base.SetChildren(children);
        }

        public override object Clone()
        {
            return new CompetencePriorityElement(gpConfig,value);
        }

        public override string ToString()
        {
            return (value is string) ? (string)value : "dummy";
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            string plan = string.Empty;
            elements = (elements is Dictionary<string, string>) ? elements : new Dictionary<string, string>();

            // taking appart the senses and putting them into the right form


            foreach (decimal elem in children)
            {
                plan += "\t(" + m_Chromosome.GetGene(elem).ToSerialize(elements) + ")";
            }

            return plan;
        }

        
    }
}
