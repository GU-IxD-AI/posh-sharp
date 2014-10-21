using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;

namespace GrammarGP.elements.POSH
{
    class DrivePriorityElement : AGene
    {
        public DrivePriorityElement(Configuration config,object value) 
            : base (config, GeneType.DrivePriorityElement, ReturnType.Bool, value)

        {
            childenReturnType = ReturnType.Bool;
        }

        public DrivePriorityElement(IChromosome chrom, Configuration config, object value)
            : this(config, value)
        {
            SetChromosome(chrom);
        }
        
        public override bool SetChildren(AGene[] children)
        {
            // children can only be DriveElements
            childTypes.Clear();

            foreach (DriveElement elem in children)
                if (elem.type == GeneType.DriveElement)
                {
                    childTypes.Add(GeneType.DriveElement);
                }
    
            return (children.Length == this.childTypes.Count) ? base.SetChildren(children) : false;
        }

        public override object Clone()
        {
            return new DrivePriorityElement(gpConfig,value);
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

        public override void Mutate(float mutation)
        {
            throw new NotImplementedException();
        }
    }
}
