using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;
using POSH.sys;

namespace GrammarGP.elements.POSH
{
    class Competence : AGene
    {
        public Competence(Configuration config,object value) 
            : base (config, GeneType.Competence, ReturnType.Bool,value)

        {
            childenReturnType = ReturnType.Bool;
        }

        public Competence(IChromosome chrom, Configuration config, object value)
            : this(config, value)
        {
            SetChromosome(chrom);
        }
        
        public override bool SetChildren(AGene[] children)
        {
            // children can be Actions, APs and Competences as one child
            // the other two are a terminal for maxRetries and a Goal for triggering
            List<AGene> childList = new List<AGene>();
            List<GeneType> childTypes = new List<GeneType>();

            this.childTypes.Clear();
            this.children.Clear();

            childList[0] = null;
            childTypes[0] = GeneType.None;

            for (int i = 0; i < children.Length; i++)
                switch (children[i].type)
                {
                    case GeneType.CompetencePriorityElement:
                        if (children[i] is CompetencePriorityElement)
                        {
                            childTypes[i+1] = GeneType.CompetencePriorityElement;
                            childList[i+1] = children[i];
                        }
                        break;

                    case GeneType.Goal:
                        if (children[i] is Goal)
                        {
                            childTypes[0] = GeneType.Goal;
                            childList[0] = children[i];
                        }
                        break;
                    default:
                        return false;
                }
            
            for (int i = 0; i < childList.Count; i++)
                if (m_Chromosome.AddGene(childList[i]))
                {
                    this.childTypes[i] = childTypes[i];
                    this.children[i] = childList[i].id;
                }
                else if (m_Chromosome == childList[i].m_Chromosome)
                {
                    childList[i].parent = this.id;
                    this.children.Add(childList[i].id);
                }

            return true;
        }

        public override object Clone()
        {
            return new Competence(gpConfig, value);
        }

        public override string ToString()
        {
            return (value is string) ? (string)value : "dummy";
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            string plan = ToString();
            string c;
            elements = (elements is Dictionary<string, string>) ? elements : new Dictionary<string, string>();

            // taking appart the senses and putting them into the right form
            if (elements.ContainsKey(plan))
                return plan;

            AGene goal = null;

            string acts = string.Empty;
            foreach (decimal elem in children)
            {
                AGene cpe = m_Chromosome.GetGene(elem);
                if (cpe.type == GeneType.CompetencePriorityElement)
                    acts += "\t(" + cpe.ToSerialize(elements) + "\t)\n";
                else if (cpe.type == GeneType.Goal)
                    goal = cpe;
            }
            
            // TODO: the current implementation does not support timeouts
            c = String.Format("(C {0} {1} (goal {3})\n\t(elements \n{2} \n\t)\n)", plan, "", acts, goal.ToSerialize(elements));
            elements[plan] = c;
            return plan;
        }

        public override AGene Mutate(float mutation)
        {
            // actions are represented as basic strings so mutating them is not possible at this level 
            // if actions should be changed xover is needed
            List<decimal> existing = new List<decimal>();
            AGene[] pool = new AGene[0];

            //if (mutation < 0.75f)
            // TODO: we could modify the children and delete/add or rearrange them with a low percentage which you correlate well with mutation
            existing.AddRange(m_Chromosome.GetAllInterChangeableGenes(type, returnType, false));
            existing.AddRange(m_Chromosome.GetAllInterChangeableGenes(type, returnType, true));

            int pick = (int)MutateNumber(mutation, new Tuple<double, double>(0, existing.Count));

            return (AGene)m_Chromosome.GetGene(existing[pick]).Clone();
        }        
    }
}
