using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.elements;

namespace GrammarGP.operators
{
    public class MaxFitnessEvaluator : IFitnessEvaluator
    {
        public bool IsFitter(object one, object two)
        {
            if (one is float)
                return (one is float) && (two is float) ? IsFitter((float)one,(float) two) : false;
            if (one is double)
                return (one is double) && (two is double) ? IsFitter((double)one, (double)two) : false;
            if (one is decimal)
                return (one is decimal) && (two is decimal) ? IsFitter((decimal)one, (decimal)two) : false;
            if (one is int)
                return (one is int) && (two is int) ? IsFitter((float)one, (float)two) : false;

            return false;
        }

        private bool IsFitter(float one, float two)
        {
            return one < two;
        }

        private bool IsFitter(double one, double two)
        {
            return one < two;
        }

        private bool IsFitter(decimal one, decimal two)
        {
            return one < two;
        }

		public int Compare(IProgram program1, IProgram program2)
		{
			if (IsFitter (program1, program2))
				return -1;
			else if (IsFitter (program2, program1))
				return 1;

			return 0;
		}
    }
}
