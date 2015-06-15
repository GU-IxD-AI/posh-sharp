using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using POSH.sys.parse;
using System.Text.RegularExpressions;

namespace POSH.testing.validate.sys.parse
{
    [TestFixture]
    class LAPLexerTest
    {
        LAPLexer lexer;

        string input = @"( 
                (C follow-player (minutes 10) (goal((fail)))
                    (elements
                      ((close-enough (trigger ((close-to-player))) stop-bot))
                      ((move (trigger ((see-player))) move-player))
                    )
                  )
                  (C wander-around (minutes 10) (goal((see-player)))
                    (elements
                      ((stuck (trigger ((is-stuck))) avoid))
                      ((pickup (trigger ((see-item))) pickup-item))
                      ((walk-around (trigger ((is-rotating 0))) walk))
                    )
                  )
                      
                  (AP avoid (minutes 10) (stop-bot rotate then-walk))
                      
                  (C then-walk (minutes 10) (goal((is-walking)))
                    (elements
                      ((try-walk (trigger ((is-rotating 0))) walk))
                    )
                  )
                      

                  (RDC life (goal ((fail)))
                      (drives
                        ((hit (trigger((hit-object)(is-rotating 0))) avoid))
                        ((follow (trigger((see-player))) follow-player))
                        ((wander (trigger((succeed))) wander-around))
                      )
                  )
                )";

        [SetUp]
        public void LexerCreation()
        {
            
            lexer = new LAPLexer(input);
        }
        [Test] 
        public void TestingPatterns()
        {
            Match match = LAPLexer.FULLTOKENS.First.Match(input);
            
            if (match is Match && match.Success)
            {
                string matchedString = match.Value;
                input = input.Substring(matchedString.Length);
                // count the number of newlines in the matched
                // string to keep track of the line number
                Console.Out.WriteLine( LAPLexer.newlines.Matches(matchedString).Count);
                Console.Out.WriteLine("match: "+ matchedString);
            }
        }
        
        [Test]
        public void TestingLines()
        {
            Console.Out.WriteLine(lexer.getLineNumber());
        }

        [Test]
        public void RetrieveToken()
        {
            lexer.token();
        }

        [Test]
        public void RetrieveAllTokens()
        {
            Token tok = lexer.token();
            int i = 0;
            while (tok is Token)
            {
                Console.Out.WriteLine("line: "+lexer.getLineNumber());
                Console.Out.WriteLine(string.Format("Token {0}: {1}",i, tok.token));
                Console.Out.WriteLine(string.Format("Token {0} value: {1}",i, tok.value));
                tok = lexer.token();
                i++;
            }

        }

        [Test]
        public void ResetInput()
        {
            Console.Out.WriteLine("line: " + lexer.getLineNumber());
            lexer.setInput(input);
            Console.Out.WriteLine("Reset input.");
            Console.Out.WriteLine("line: " + lexer.getLineNumber());
            Console.Out.WriteLine(string.Format("Token {0}", lexer.token().token));
        }

    }
}
