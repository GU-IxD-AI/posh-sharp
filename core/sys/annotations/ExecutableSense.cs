using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys.annotations
{
    public class ExecutableSense : Attribute
    {
        public string command { get; private set; }
        public float version { get; private set; }

		public ExecutableSense(string command) : this(command, 0.1f)
		{}

        //TODO: remodel the Action to link it against any method which is called by ExecutableSense
        public ExecutableSense(string command, float version)
        {
            this.command = command;
            this.version = version;
        }
    }
}
