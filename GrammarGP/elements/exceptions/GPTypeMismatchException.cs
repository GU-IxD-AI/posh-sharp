using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarGP.elements.exceptions
{
    public class GPTypeMismatchException : Exception
    {
        public GPTypeMismatchException(string message)
            : base(message)
        {
        }
        public GPTypeMismatchException()
            : base()
        {
        }
    }
}
