using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using GrammarGP.env;

namespace GrammarGP.elements.POSH
{
    public class Terminal:AGene,IComparable
    {
        Tuple<double, double> bounds;

        public Terminal(Configuration config, ReturnType a_type, object value)
            : base(config, GeneType.Terminal, a_type,value)
        {
            bounds = null;
        }

        public Terminal(IChromosome chrom, Configuration config, ReturnType a_type, object value)
            : this(config, a_type, value)
        {
            SetChromosome(chrom);
        }
        public Terminal(Configuration config,ReturnType a_type)
            : this(config,a_type, null)
        {
            
            switch (a_type)
            {
                case ReturnType.Bool:
                    value = (config.randomGenerator.Next(1) > 0) ? true : false;
                    break;
                case ReturnType.Text:
                    value = "none";
                    break;
                case ReturnType.Number:
                    value = config.randomGenerator.NextDouble();
                    break;
            }
        }

        /// <summary>
        /// Sets the bounds for the Terminal between which it can mutate. 
        /// If the terminal is a string, the bounds present the length of the resulting string.
        /// It is also assumed that the string is within the range of [az]
        /// If the terminal is a number, the bounds present the lowest and higherst possible value to mutate to.
        /// If SetRange is not used for a number the terminal is treated as a percentage value between [0d,1.0d]
        /// </summary>
        /// <param name="lower"></param>
        /// <param name="upper"></param>
        /// <returns></returns>
        public bool SetRange(double lower, double upper)
        {
            if (returnType != ReturnType.Number || returnType != ReturnType.Text)
                return false;
            bounds = new Tuple<double, double>(lower, upper);

            return true;
        }

        public override object Clone()
        {
            //@TODO: still need to finish this in the end
            Terminal clone = new Terminal(gpConfig, returnType, value);
            clone.bounds = bounds;
            
            return clone;
        }

        public override string ToString()
        {
            return value.ToString();
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            return value.ToString();
        }

        public override void Mutate(float mutation)
        {
            switch (returnType)
            {
                case ReturnType.Bool:
                    if (value is bool)
                        value = MutateBool(mutation);
                    break;
                case ReturnType.Number:
                    if (value is double)
                        if (bounds is Tuple<double, double>)
                            value = MutateNumber(mutation, bounds);
                        else
                            value = MutateNumber(mutation, new Tuple<double, double>(0, 1.0));
                    break;
                case ReturnType.Text:
                    break;
                default:
                    break;
            }
        }

        private object MutateBool(float mutation)
        {
            return (mutation < 0.5f) ? true : false;
        }

        private double MutateNumber(float mutation, Tuple<double, double> range)
        {
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

            double number = (double) value;
            double mutationRange = mutation*2 * (range.Second - range.First);

            if (increaseValue)
                number = (number + mutationRange > range.Second) ? (number + mutationRange) - range.Second + range.First : number + mutationRange;
            else
                number = (number - mutationRange < range.First) ? range.Second - Math.Abs(range.First - Math.Abs(number - mutationRange)) : number - mutationRange;

            return number;
        }

        public int CompareTo(object obj)
        {
            switch (returnType)
            {
                case ReturnType.Number:
                    if (obj is double)
                        return (int)((double)value - (double)obj);
                    break;
                case ReturnType.Text:
                    if (obj is string)
                        return ((string)value).CompareTo((string)obj);
                    break;
                default:
                    break;
            }

            return 0;
        }
    }
}
