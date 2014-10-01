using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys.annotations
{
    public class ExecutableAction : POSHPrimitive
    {
        public ExecutableAction(string command) : base(command)
		{}

        //TODO: remodel the Action to link it against any method which is called by ExecutableAction
        public ExecutableAction(string command, float version)
            : base(command, version)
        { }


        public override bool Equals(object obj)
        {
            ExecutableAction otherAction = obj as ExecutableAction;
            
            if (obj == null || otherAction == null)
                return false;

            return (otherAction.command == this.command) ? true : false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
