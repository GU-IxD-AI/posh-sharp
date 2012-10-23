using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys.annotations
{
    public class ExecutableAction : Attribute
    {
        public string command { get; private set; }

        //TODO: remodel the Action to link it against any method which is called by ExecutableAction
        public ExecutableAction(string command)
        {
            this.command = command;
        }
    }
}
