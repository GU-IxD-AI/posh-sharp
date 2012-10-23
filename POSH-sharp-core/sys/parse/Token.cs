using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys.parse
{
    /// <summary>
    /// A single token.
    /// </summary>
    public class Token
    { 
            
        public string token;
        public string value;

        /// <summary>
        /// Initilaises the token with a token-name and a value.
        /// </summary>
        /// <param name="token">The name (type) of the token.</param>
        /// <param name="value">The value of the token.</param>
        public Token(string token,string value)
        {
            this.token = token;
            this.value = value;
        }
    }
}
