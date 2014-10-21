using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;


namespace GrammarGP.elements.POSH
{
    class CompetenceElement : AGene
    {
        public CompetenceElement(Configuration config,object value) 
            : base (config, GeneType.CompetenceElement, ReturnType.Bool,value)

        {
            childenReturnType = ReturnType.Bool;
        }

        public CompetenceElement(IChromosome chrom, Configuration config, object value) 
            : this (config, value)
        {
            SetChromosome(chrom);
        }
        public override bool SetChildren(AGene[] children)
        {
            // children can be Actions, APs and Competences as one child
            // the other two are a terminal for maxRetries and a Goal for triggering
            if (children.Length != 3)
                return false;

            AGene[] childList = new AGene[3];


            int checksum = 0;
            
            for (int i = 0; i < children.Length; i++)
                switch (children[i].type)
                {
                    case GeneType.Action :
                        if (children[i] is POSHAction)
                        {
                            childTypes[1] = GeneType.Action;
                            childList[1] = children[i];
                            checksum += 1;
                        }
                        break;
                    case GeneType.ActionPattern:
                        if (children[i] is ActionPattern)
                        {
                            childTypes[1] = GeneType.ActionPattern;
                            childList[1] = children[i];
                            checksum += 1;
                        }
                        break;
                    case GeneType.Competence:
                        if (children[i] is Competence)
                        {
                            childTypes[1] = GeneType.Competence;
                            childList[1] = children[i];
                            checksum += 1;
                        }
                        break;

                    case GeneType.Goal:
                        if (children[i] is Goal)
                        {
                            childTypes[0] = GeneType.Goal;
                            childList[0] = children[i];
                            checksum += 2;
                        }
                        break;
                    case GeneType.Terminal:
                        if (children[i] is Terminal)
                        {
                            childTypes[2] = GeneType.Terminal;
                            childList[2] = children[i];
                            checksum += 4;
                        }
                        break;
                    default:
                        return false;
                }
            
            return (checksum == 7) ? base.SetChildren(childList) : false;
        }

        public override object Clone()
        {
            return new CompetenceElement(gpConfig, value);
        }

        public override string ToString()
        {
            return (value is string) ? (string)value : "dummy";
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            string plan = ToString();
            elements = (elements is Dictionary<string, string>) ? elements : new Dictionary<string, string>();

            // taking appart the senses and putting them into the right form
            if (children.Count != 3)
                return string.Empty;

            plan = String.Format("({0} (trigger {1}) {2} {3})", plan, m_Chromosome.GetGene(children[0]).ToSerialize(elements), m_Chromosome.GetGene(children[1]).ToSerialize(elements), m_Chromosome.GetGene(children[2]).ToSerialize(elements));


            return plan;
        }

        public override void Mutate(float mutation)
        {
            throw new NotImplementedException();
        }
    }
}
