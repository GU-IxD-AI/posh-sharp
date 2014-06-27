using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys.annotations
{
    public class ExecutableAction : Attribute
    {
        public string command { get; private set;}
        public float version { get; private set; }

		public ExecutableAction(string command) : this(command, 0.1f)
		{}

        //TODO: remodel the Action to link it against any method which is called by ExecutableAction
        public ExecutableAction(string command, float version)
        {
            this.command = command;
            this.version = version;
        }
    }
}
