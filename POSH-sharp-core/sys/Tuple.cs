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
        public Tuple(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }
    }

    public class Tuple<T1, T2,T3> : Tuple<T1,T2>
    {
        public T3 Third { get; private set; }
        public Tuple(T1 first, T2 second, T3 third)
            : base(first,second)
        {
            Third = third;
        }
    }

    public class Tuple<T1, T2, T3, T4>: Tuple<T1,T2,T3>
    {
        public T4 Forth { get; private set; }

        public Tuple(T1 first, T2 second, T3 third, T4 forth)
            : base(first,second,third)
        {
            Forth = forth;
        }
    }

    public class Tuple<T1, T2, T3, T4, T5> : Tuple<T1, T2, T3, T4>
    {
        public T5 Fifth { get; private set; }

        public Tuple(T1 first, T2 second, T3 third, T4 forth, T5 fifth)
            : base(first, second, third,forth)
        {
            Fifth = fifth;
        }
    }

    public class Tuple<T1, T2, T3, T4, T5, T6> : Tuple<T1, T2, T3, T4, T5>
    {
        public T6 Sixth { get; private set; }

        public Tuple(T1 first, T2 second, T3 third, T4 forth, T5 fifth, T6 sixth)
            : base(first, second, third, forth, fifth)
        {
            Sixth = sixth;
        }
    }

    public class Tuple<T1, T2, T3, T4, T5, T6, T7> : Tuple<T1, T2, T3, T4, T5, T6>
    {
        public T7 Seventh { get; private set; }

        public Tuple(T1 first, T2 second, T3 third, T4 forth, T5 fifth, T6 sixth, T7 seventh)
            : base(first, second, third, forth, fifth, sixth)
        {
            Seventh = seventh;
        }
    }

   


}
