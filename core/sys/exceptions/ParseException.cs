using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys.exceptions
{
    /// <summary>
    /// An Exception that indicates a parse error.
    /// </summary>
    public class ParseException : Exception
    {
        public ParseException() {}
        public ParseException(string message) : base(message) {}
        public ParseException(string message, System.Exception inner) : base(message,inner) { }

        // Constructor needed for serialization 
        // when exception propagates from a remoting server to the client.
        protected ParseException(System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context) : base(info,context) {}
        
    }
}
