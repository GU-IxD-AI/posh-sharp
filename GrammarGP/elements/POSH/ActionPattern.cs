using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;

namespace GrammarGP.elements.POSH
{
    class ActionPattern : AGene
    {
        public ActionPattern(Configuration config,object value) 
            : base (config, GeneType.ActionPattern, ReturnType.Bool,value)

        {
            childenReturnType = ReturnType.Bool;
        }
        public ActionPattern(IChromosome chrom, Configuration config, object value)
            : this(config, value)
        {
            SetChromosome(chrom);
        }

        public override string ToString()
        {
            return (value is string) ? (string)value : "dummy";
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            string plan = ToString();
            string ap;
            elements = (elements is Dictionary<string, string>) ? elements : new Dictionary<string, string>();

            // taking appart the senses and putting them into the right form
            if (elements.ContainsKey(plan))
                return plan;


            string acts = string.Empty;
            foreach (decimal elem in children)
            {
                acts += "\t" + m_Chromosome.GetGene(elem).ToSerialize(elements) + "\n";
            }
            // TODO: the current implementation does not support timeouts
            ap = String.Format("(AP {0} {1} ( \n{2} \n))", plan, "", acts);
            elements[plan] = ap;
            
            return plan;
        }

        public override bool SetChildren(AGene[] children)
        {
            // if I want to shift POSH closer to BT it is possible to treat APs as sequence nodes allowing other types of genes as well
            for (int i = 0; i < children.Length; i++)
                if (children[i] is POSHAction)
                    childTypes.Add(GeneType.Action);
            
            return (childTypes.Count == children.Length) ? base.SetChildren(children) : false;
        }

        public override object Clone()
        {
            return new ActionPattern(gpConfig,value);
        }

        public override void Mutate(float mutation)
        {
            //@TODO:  THe idea might be to use the config to access all available senses and add or remove some from the children
            // the same will apply to all elements of a class to be able to mutate into a different element of the same group
            throw new NotImplementedException();
        }
    }
}
