using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys.annotations
{
    public class POSHPrimitive: Attribute
    {
        /// <summary>
        /// the command is the name of a plan element referencing a specific method
        /// </summary>
        public string command { get; private set; }

        /// <summary>
        /// the linked method name is used to identify the correct method name inside a specific behaviour
        /// </summary>
        public string linkedMethod { get; private set; }

        /// <summary>
        /// the linked method name is used to identify the correct method name inside a specific behaviour
        /// </summary>
        public Behaviour orginatingBehaviour { get; private set; }

        /// <summary>
        /// the verison number is used to differentiate between different maturety levels of the underlying method
        /// the default value is 0.1f
        /// </summary>
        public float version { get; private set; }

        public POSHPrimitive(string command) : this(command, 0.1f) { }

        public POSHPrimitive(string command, float version)
        {
            this.command = command;
            this.version = version;
            linkedMethod = "";
            orginatingBehaviour = null;
        }

        /// <summary>
        /// used to link a method to a specific plan action. As multiple method can reference 
        /// different version of the same plan element a one to one matching is required using different version numbers
        /// </summary>
        /// <param name="name"></param>
        internal void SetLinkedMethod(string name)
        {
            linkedMethod = name;
        }

        /// <summary>
        /// used to link the behaviour the prmitive originates from
        /// different versions of a plan element might be originating from different behaviours
        /// </summary>
        /// <param name="name"></param>
        internal void SetOriginatingBhaviour(Behaviour behave)
        {
            orginatingBehaviour = behave;
        }

        public override int GetHashCode()
        {
            return command.GetHashCode()*version.GetHashCode();
        }
    }
}
