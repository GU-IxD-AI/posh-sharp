using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys.annotations
{
    public class ExecutableSense : POSHPrimitive
    {
        public ExecutableSense(string command) : base(command)
		{}

        //TODO: remodel the Action to link it against any method which is called by ExecutableSense
        public ExecutableSense(string command, float version)
            : base(command,version)
        { }

        public override bool Equals(object obj)
        {
            ExecutableSense otherSense = obj as ExecutableSense;

            if (obj == null || otherSense == null)
                return false;

            return (otherSense.command == this.command) ? true : false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
