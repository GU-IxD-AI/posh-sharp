using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;

namespace GrammarGP.elements.POSH
{
    public class DriveCollection : AGene
    {
        private Goal goal;
        private string driveType;

        public DriveCollection(Configuration config,object value,string drive) 
            : base (config, GeneType.DriveCollection, ReturnType.Bool,value)

        {
            childenReturnType = ReturnType.Bool;
            driveType = drive;
            goal = null;
        }

        public DriveCollection(IChromosome chrom, Configuration config, object value,string drive)
            : this(config, value,drive)
        {
            SetChromosome(chrom);
        }
        
        public override bool SetChildren(AGene[] children)
        {
            // children can be Actions, APs and Competences as one child
            // the other two are a terminal for maxRetries and a Goal for triggering
            this.childTypes.Clear();
            List<AGene> childList = new List<AGene>();

            for (int i = 0; i < children.Length; i++)
                switch (children[i].type)
                {
                    case GeneType.DrivePriorityElement:
                        if (children[i] is DrivePriorityElement)
                        {
                            childTypes[i+1] = GeneType.DrivePriorityElement;
                            childList[i+1] = children[i];
                        }
                        break;

                    case GeneType.Goal:
                        if (children[i] is Goal)
                        {
                            childList[0] = (Goal)children[i];
                            childTypes[0] = GeneType.Goal;
                        }
                        break;
                    default:
                        return false;
                }

            return base.SetChildren(childList.ToArray());
        }

        public override object Clone()
        {
            return new DriveCollection(gpConfig, value,driveType);
        }

        public override string ToString()
        {
            return (value is string) ? (string)value : "dummy";
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            string dc;
            elements = (elements is Dictionary<string, string>) ? elements : new Dictionary<string, string>();


            string acts = string.Empty;
            foreach (decimal elem in this.children)
            {
                acts += "\t(" + m_Chromosome.GetGene(elem).ToSerialize(elements) + "\t)\n";
            }

            // TODO: the current implementation does not support timeouts
            dc = String.Format("({0} {1} (goal {3})\n\t(drives \n{2} \n\t)\n)", driveType, ToString(), acts, goal.ToSerialize(elements));

            return dc;
        }

        public override void Mutate(float mutation)
        {
            throw new NotImplementedException();
        }
    }
}
