using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys.exceptions
{
    public class AgentInitParseException : Exception
    {
        public AgentInitParseException() {}
        public AgentInitParseException(string message) : base(message) {}
        public AgentInitParseException(string message, System.Exception inner) : base(message,inner) { }

        // Constructor needed for serialization 
        // when exception propagates from a remoting server to the client.
        protected AgentInitParseException(System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context) : base(info,context) {}
    
        
    }
}
