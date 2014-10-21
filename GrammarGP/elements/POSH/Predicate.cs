using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;

namespace GrammarGP.elements.POSH
{
    public class Predicate : AGene
    {
        public enum PredicateType {LESS=0,LESSEQUAL=1,EQUAL=2,NOTEQUAL=3,MOREEQUAL=4,MORE=5 }

        public Predicate(Configuration config,object value)
            : base(config, GeneType.Predicate, ReturnType.Void) 
        {
            if (value is PredicateType)
                this.value = value;

        }

        public Predicate(IChromosome chrom, Configuration config, object value)
            : this(config, value)
        {
            SetChromosome(chrom);
        }


        public override object Clone()
        {
            return new Predicate(gpConfig, value);
        }

        public override string ToString()
        {
            if (value is PredicateType)
                switch ((PredicateType)value)
                {
                    case PredicateType.LESS:
                        return "<";
                    case PredicateType.LESSEQUAL:
                        return "<=";
                    case PredicateType.EQUAL:
                        return "==";
                    case PredicateType.NOTEQUAL:
                        return "!=";
                    case PredicateType.MOREEQUAL:
                        return ">=";
                    case PredicateType.MORE:
                        return ">=";

                }



            return string.Empty;
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            return this.ToString();
        }

        public override void Mutate(float mutation)
        {
            // possible options for a predicate starting to count at zero
            int range = 5;
            if ( !(value is PredicateType) )
                return;
            int position = (int)value;
            
            bool increaseValue = false;

            if (mutation < 0.5f)
                //decrease number
                increaseValue = false;
            else
            {
                //increase number
                increaseValue = true;
                mutation = mutation - 0.5f;
            }

            // the distance ranges from the first to the last possible predicate and mutation*2  is spanning from [0,1]
            double mutationRange = mutation * 2 * range;

            if (increaseValue)
                position = (int) ( (position + mutationRange > range) ? (position + mutationRange) - range : position + mutationRange );
            else
                position = (int) ( (position - mutationRange < range) ? range - Math.Abs(position - mutationRange) : position - mutationRange );

            value = position;



        }
    }
}
