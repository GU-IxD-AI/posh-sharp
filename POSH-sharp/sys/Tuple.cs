using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys
{
    public class Tuple<T1, T2>
    {
        public T1 First { get; private set; }
        public T2 Second { get; private set; }
        internal Tuple(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
    }

    public class Tuple<T1, T2,T3> : Tuple<T1,T2>
    {
        public T3 Third { get; private set; }
        internal Tuple(T1 first, T2 second, T3 third)
            : base(first,second)
        {
            Third = third;
        }
    }

    public class Tuple<T1, T2, T3, T4>: Tuple<T1,T2,T3>
    {
        public T4 Forth { get; private set; }
        
        internal Tuple(T1 first, T2 second, T3 third, T4 forth)
            : base(first,second,third)
        {
            Forth = forth;
        }
    }

   


}
