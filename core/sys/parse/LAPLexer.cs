using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace POSH.sys.parse
{
    /// <summary>
    /// A Lexer for tokenising .lap files.
    /// 
    /// This lexer is used by LAPParser to tokenise the input string.
    /// </summary>
    public class LAPLexer
    {
        /// <summary>
        /// preprocessing pattern. Everything that they match is
        /// substituted by the second string in the pair.
        /// </summary>
        public static Tuple<Regex,string> SUBPATTERN =  new Tuple<Regex,string>(new Regex("(#|;)[^\n]*"),"");



        /// <summary>
        ///  tokens that match fully, independent of what follows after
        ///  them. These are tokens that don't need to be separated by
        ///  separating characters. This doesn't work for reserved words,
        ///  as they would match even if they only match the beginning
        ///  of a word.
        /// </summary>
        public static Tuple<Regex, string> FULLTOKENS = new Tuple<Regex, string>(new Regex("^\"([A-Z|a-z][A-Z|a-z| ]*[A-Z|a-z]+)\""), "COMMENT");

        /// <summary>
        /// separating characters are characters that split the input
        /// string into tokens. These will be ignored, if they are not
        /// in char_tokens.
        /// </summary>
        public static char[] SEPARATINGCHARS =  new char[] {' ','(',')','\n','\r','\t'};

        /// <summary>
        /// character tokens are tokens that are represented by single
        /// characters. This has to be a subset of separating_chars.
        /// </summary>
        public static Dictionary<char,string> CHARTOKENS =  new Dictionary<char,string> {
            {'(', "LPAREN"},
            {')', "RPAREN"}
        };

        /// <summary>
        /// these tokens need to be spearated by separating characters
        /// and need to match the strings inbetween fully. The tokens are
        /// given in their order of priority. Hence, if several of those
        /// tokens match, the first in the list is returned.
        /// </summary>
         public static Dictionary<Regex,string> TOKENS =  new Dictionary<Regex,string> {
            {new Regex("documentation"),"DOC"},
            {new Regex("AP"),"AP"},
            {new Regex("C"),"C"},
            {new Regex("DC"),"DC"},
            {new Regex("RDC"),"RDC"},
            {new Regex("SDC"),"SDC"},
            {new Regex("SRDC"),"SRDC"},
            {new Regex("nil"),"NIL"},
            {new Regex("(?i)drives"),"DRIVES"},
            {new Regex("(?i)elements"),"ELEMENTS"},
            {new Regex("(?i)trigger"),"TRIGGER"},
            {new Regex("(?i)goal"),"GOAL"},
            {new Regex("(?i)hours"),"HOURS"},
            {new Regex("(?i)minutes"),"MINUTES"},
            {new Regex("(?i)seconds"),"SECONDS"},
            {new Regex("(?i)hz"),"HZ"},
            {new Regex("(?i)pm"),"PM"},
            {new Regex("(?i)none"),"NONE"},
            {new Regex("(==|!=|=|<=|>=|<|>)"),"PREDICATE"},
            {new Regex(@"-?(\d*\.\d+|\d+\.)([eE][\+\-]?\d+)?"),"NUMFLOAT"},
            {new Regex(@"\-?[0-9]+"),"NUMINT"},
            {new Regex(@"(?i)[a-z][a-z0-9_\-]*"),"NAME"},
            {new Regex(@"(?i)'?[a-z][a-z0-9_\-]*"),"STRINGVALUE"}
        };

        // to count the number of newlines
        public static char newline = '\n';
        public static Regex newlines = new Regex("\n");


        private string input;
        private int lineNo;

        /// <summary>
        /// Initialises the lexer with the given input string.
        /// </summary>
        /// <param name="inputString">An input string.</param>
        public LAPLexer(string inputString)
        {
            this.input = "";
            lineNo = 1;
            if (inputString is string && inputString.Length > 1)
                setInput(inputString);
        }

        public void setInput(string inputString)
        {
            inputString = SUBPATTERN.First.Replace(inputString,SUBPATTERN.Second);
            input = inputString;
            lineNo = 1;
        }

        private Token checkFullTokens()
        {
            // first check for full tokens
            Match match = FULLTOKENS.First.Match(input.Trim());
            if (match is Match && match.Success)
            {
                string matchedString = match.Groups[1].Value;
                input = input.Substring(match.Value.Length+1);
                // count the number of newlines in the matched
                // string to keep track of the line number
                lineNo += newlines.Matches(matchedString).Count;
                return new Token(FULLTOKENS.Second, matchedString);
            }
            return null;

        }

        private Token checkNormalTokens()
        {                
            // none of the separating characters matched
            // let's split the string and check for normal tokens
            int sepPos = -1;
            // find the closest separating character
            foreach (char sepChar in SEPARATINGCHARS)
            {
                int pos = input.IndexOf(sepChar);
                if (pos >= 0 && (sepPos == -1 || pos < sepPos))
                    sepPos = pos;
            }
            // take the full string if no separating character was found
            string sepString;
            if (sepPos == -1)
                sepString = input;
            else
            {
                sepString = input.Substring(0,sepPos);
                // find the first fully matching token
                foreach (KeyValuePair<Regex,string> tk in TOKENS)
                {
                    Match match  = tk.Key.Match(sepString);
                    if (match is Match && match.Success && match.Value.Length == sepString.Length)
                    {
                        string matchedString = match.Value;
                        input  = input.Substring(matchedString.Length);
                        // count the number of newlines in the matched
                        // string to keep track of the line number
                        lineNo += newlines.Matches(matchedString).Count;
                        return new Token(tk.Value, matchedString);
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Returns the next found token in the input string.
        /// 
        /// If the input string is empty, then None is returned.
        /// </summary>
        /// <returns>Next token.</returns>
        public Token token()
        {
            while (input is string && input.Length > 0)
            {

                Token result = checkFullTokens();
                if (result is Token)
                    return result;

                // none of the full tokens matched
                // proceed with checking for single characters
                char singleChar = input[0];

                if (SEPARATINGCHARS.Contains(singleChar))
                {
                    input = input.Substring(1);
                    if (singleChar == newline)
                        lineNo++;
                    if (CHARTOKENS.ContainsKey(singleChar))
                        return new Token(CHARTOKENS[singleChar],singleChar.ToString());
                    // continue with next charater in input string
                    continue;
                }

                result = checkNormalTokens();
                if (result is Token)
                    return result;

                // no token matched: give error over single character
                char charString = input[0];
                input = input.Substring(1);
                error(charString);
            }
            // the input string is empty
            return null;
        }

        /// <summary>
        /// Returns the current line number.
        /// </summary>
        /// <returns>The current line number.</returns>
        public int getLineNumber()
        {
            return lineNo;
        }

        /// <summary>
        /// Report an illegal character.
        /// </summary>
        /// <param name="stringElement">The illegal character.</param>
        public void error(char stringElement)
        {
            Console.Out.WriteLine(string.Format("Line {0}: Illegal character '{1}' found", lineNo, stringElement));
        }
    }
}
