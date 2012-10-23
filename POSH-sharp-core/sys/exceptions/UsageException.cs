using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys.exceptions
{
    public class UsageException : Exception
    {
        public UsageException() {}
        public UsageException(string message) : base(message) {}
        public UsageException(string message, System.Exception inner) : base(message,inner) { }

        // Constructor needed for serialization 
        // when exception propagates from a remoting server to the client.
        protected UsageException(System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context) : base(info,context) {}
    
        
    }
}
