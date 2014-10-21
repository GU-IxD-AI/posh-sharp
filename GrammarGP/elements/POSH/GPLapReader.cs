using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.parse;

namespace GrammarGP.elements.POSH
{
    public class GPLapReader : LAPParser
    {
        public GPLapReader()
            : base()
        { }

        /// <summary>
        /// Parses the given input and returns a plan builder object.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>The plan builder object representing the plan.</returns>
        public new GPPlanBuilder Parse(string inputString)
        {
            lex = new LAPLexer(inputString);
            return Start();
        }
        
        /// <summary>
        /// The parser start symbol.
        /// 
        /// When called, it parses the set input string and returns
        /// the created plan builder object.
        /// </summary>
        /// <returns>A plan builder object representing the parsed plan.</returns>
        public new GPPlanBuilder Start()
        {
            NextToken();
            return Plan();
        }

        /// <summary>
        /// plan ::= "(" [ "(" <docstring> ]
        ///                ( ( "(" <competence> | <action-pattern> )*
        ///                 "(" <drive-collection>
        ///                 ( "(" <competence> | <action-pattern> )*
        ///               )
        ///                | ( "(" <competence> )
        ///                | ( "(" <action-pattern> )
        ///            ")"
        /// </summary>
        /// <returns>A plan builder object representing the parsed plan.</returns>
        protected new internal GPPlanBuilder Plan()
        {
            GPPlanBuilder planBuilder = new GPPlanBuilder();
            // this method cheats a bit by counting the action-pattern
            // and competences and also drive-collections to check when things are
            // allowed were.
            if (!Match(new string[] { "LPAREN", }))
                Error(string.Format("Plan needs to start with '(' rather than '{0}'", token.value));
            NextToken();
            // action pattern, competence, docstring, drive collection
            int ap = 0, c = 0, d = 0, dc = 0;
            while (true)
            {
                if (!Match(new string[] { "LPAREN", "RPAREN" }))
                    Error(string.Format("Expected '(' as start of documentation / " +
                        "competence / action-pattern / drive-collection, or " +
                        "')' to end plan, instead of '{0}'", token.value));
                if (Match(new string[] { "RPAREN", }))
                {
                    // end of plan
                    NextToken();
                    break;
                }
                NextToken();
                // check for documentation
                if (Match(new string[] { "DOC", }))
                {
                    if (ap + c + dc + d > 0)
                        Error("Documentation only allowed as first " +
                            "element in plan");
                    d += 1;
                    planBuilder.setDocString(GetDocString());
                    // print docString();
                }
                // check for competence
                else if (Match(new string[] { "C", }))
                {
                    c++;
                    planBuilder.addCompetence(GetCompetence());
                    // print competence()
                }
                // check for action-pattern
                else if (Match(new string[] { "AP", }))
                {
                    ap++;
                    planBuilder.addActionPattern(GetActionPattern());
                    // print actionPattern();
                }
                // check for drive-collection
                else if (Match(new string[] { "DC", "RDC", "SDC", "SRDC" }))
                {
                    if (dc > 0)
                        Error("Only a single drive-collection allowed");
                    dc++;
                    planBuilder.SetDriveCollection(GetDriveCollection());
                    // print
                }
                else
                    Error(string.Format("Expected docstring / competence / action " +
                        "pattern or drive collection instead of '{0}'", token.value));
            }

            // the plan was closed
            if (token is Token)
                Error(string.Format("Illegal token '{0}' after end of plan", token.value));
            if (dc == 0 && (ap + c) != 1)
                Error("Illegal plan: A plan without a drive-collection " +
                    "only allows for a SINLGE action-pattern OR a SINGLE competence");

            // everything is fine
            return planBuilder;
        }
    }
}
