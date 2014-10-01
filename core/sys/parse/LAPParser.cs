using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.exceptions;
using POSH.sys.strict;



 namespace POSH.sys.parse
{
    /// <remarks>
    /// Parser for .lap files.
    /// 
    /// The parser accepts the following grammar:
    ///
    /// Preprocessing
    /// -------------
    ///   All element matching L{(\#|\;)[^\n]*} are removed. That removes
    ///   all the comments staring with '#' or ';'.
    ///
    /// Terminal Symbols
    /// ----------------
    ///   The following terminal symbols are accepted::
    ///
    ///     AP                       AP
    ///     C                        C
    ///     DC                       DC
    ///     RDC                      RDC
    ///     SDC                      SDC
    ///     SRDC                     SRDC
    ///     nil                      NIL
    ///     (?i)drives               DRIVES
    ///     (?i)elements             ELEMENTS
    ///     (?i)trigger              TRIGGER
    ///     (?i)goal                 GOAL
    ///     (?i)hours                HOURS
    ///     (?i)minutes              MINUTES
    ///     (?i)seconds              SECONDS
    ///     (?i)hz                   HZ
    ///     (?i)pm                   PM
    ///     (?i)none                 NONE
    ///     (?i)documentation        DOCUMENTATION
    ///     (==|=|!=|<|>|<=|>=)      PREDICATE
    ///     \-?(\d*\.\d+|\d+\.)([eE][\+\-]?\d+)?  NUMFLOAT
    ///     \-?[0-9]+                NUMINT
    ///     (?i)[a-z][a-z0-9_\-]*    NAME
    ///     (?i)'?[a-z][a-z0-9_\-]*  STRINGVALUE
    ///     \"[^\"]*\"                  COMMENT
    ///
    /// Production Rules
    /// ----------------
    ///   The following production rules are used::
    ///
    ///                        plan ::= "(" [ "(" <docstring> ]
    ///                                     ( ( "(" <competence> | <action-pattern> )*
    ///                                       "(" <drive-collection>
    ///                                       ( "(" <competence> | <action-pattern> )*
    ///                                     )
    ///                                     | ( "(" <competence> )
    ///                                     | ( "(" <action-pattern> )
    ///                                 ")"
    ///                   docstring ::= DOCUMENTATION COMMENT COMMENT COMMENT ")"
    ///
    ///            drive-collection ::= <drive-collection-id> NAME
    ///                                 ( NIL | "(" <goal> | )
    ///                                 "(" DRIVES <drive-priorities> ")" ")"
    ///         drive-collection-id ::= DC | RDC | SDC | SRDC
    ///            drive_priorities ::= <drive-elements>+
    ///              drive-elements ::= "(" <drive-element>+ ")"
    ///               drive-element ::= "(" NAME ( NIL | "(" <trigger> | ) NAME
    ///                                     ( NIL | "(" <freq> | ) <opt-comment> ")"
    ///
    ///                  competence ::= C NAME ( NIL | "(" <time> | )
    ///                                 ( NIL | "(" <goal> | ) "(" ELEMENTS
    ///                                 <competence-priorities> ")" <opt-comment> ")"
    ///       competence-priorities ::= <competence-elements>+
    ///         competence-elements ::= "(" <competence-element>+ ")"
    ///          competence-element ::= "(" NAME [ "(" <trigger> ] NAME [ INTNUM ]
    ///                                     <opt-comment> ")"
    ///
    ///              action-pattern ::= AP NAME ( NIL | "(" <time> | )
    ///                                 "(" <action-pattern-elements> <opt-comment> ")"
    ///     action-pattern-elements ::= ( <full-sense> | NAME )+ ")"
    ///
    ///                        goal ::= GOAL <senses> ")"
    ///                     trigger ::= TRIGGER <senses> ")"
    ///                      senses ::= ( NIL | "(" ( NAME | <full-sense> )+ ")" )
    ///                  full-sense ::= "(" NAME [<value> [<predicate>]] ")"
    ///                       value ::= NUMINT | NUMFLOAT | NAME | STRINGVALUE | NIL
    ///                   predicate ::= PREDICATE
    ///
    ///                        freq ::= <freq-unit> <numfloat> ")"
    ///                   freq-unit ::= HOURS | MINUTES | SECONDS | HZ | PM | NONE
    ///                        time ::= <time-unit> <numfloat> ")"
    ///                   time-unit ::= HOURS | MINUTES | SECONDS | NONE
    ///                    numfloat ::= NUMINT | NUMFLOAT
    ///
    ///                 opt-comment ::= COMMENT |
    /// </remarks>
    /// <summary>
    /// A recursive descent parser for .lap files.
    /// 
    /// The parser takes a single input string that represents the plan
    /// file and creates a plan builder object that it then returns.
    /// 
    /// If an error while parsing (or tokenising) is encountered,
    /// a ParseError is raised.
    /// </summary>
    public class LAPParser
    {
        public static List<E> ShuffleList<E>(List<E> inputList)
        {
             List<E> randomList = new List<E>();

             Random r = new Random();
             int randomIndex = 0;
             while (inputList.Count > 0)
             {
                  randomIndex = r.Next(0, inputList.Count); //Choose a random object in the list
                  randomList.Add(inputList[randomIndex]); //add it to the new, random list
                  inputList.RemoveAt(randomIndex); //remove to avoid duplicates
             }

             return randomList; //return the new random list
        }

        // additional members

        protected LAPLexer lex;
        protected Token token;

        /// <summary>
        /// Initialises the parser.
        /// </summary>
        public LAPParser()
        {
            lex = null;
            token = null;
        }

        /// <summary>
        /// Parses the given input and returns a plan builder object.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>The plan builder object representing the plan.</returns>
        public PlanBuilder Parse( string inputString)
        {
            lex = new LAPLexer(inputString);
            return Start();
        }

        /// <summary>
        /// Gets the next token from the lexer.
        /// </summary>
        public void NextToken()
        {
            token = lex.token();
            // if (token is Token)
            //     Console.Out.WriteLine("{0}: '{1}'", token.token, token.value);
        }

        /// <summary>
        /// Checks if the current token matches the allowed tokens.
        /// 
        /// If there is no current token, then this method raises an Exception
        /// that indicates that we've reached the end of the input (unexpectedly).
        /// 
        /// Otherwise it returns if the current token type matches any of the
        /// given token types.
        /// </summary>
        /// <param name="allowedTokens">A list of allowed tokens.</param>
        /// <returns>If the current token type matches any of the allowed tokens.</returns>
        /// <exception cref="ParseException">If there is no current token.</exception>  
        public bool Match(string[] allowedTokens)
        {
            if (!(token is Token))
                throw new ParseException("Unexpected End Of File (EOF)");
            return (allowedTokens.Contains(token.token));
        }

        /// <summary>
        /// Raises an error with the given message.
        /// 
        /// This method raises a ParseException of type
        /// 'Line xxx: [msg]'.
        /// </summary>
        /// <param name="msg">The error message.</param>
        /// <exception cref="ParseException">always</exception>
        public void Error(string msg)
        {
            throw new ParseException(string.Format("Line {0}: {1}", lex.getLineNumber(), msg));
        }

        /// <summary>
        /// The parser start symbol.
        /// 
        /// When called, it parses the set input string and returns
        /// the created plan builder object.
        /// </summary>
        /// <returns>A plan builder object representing the parsed plan.</returns>
        public PlanBuilder Start()
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
        protected internal PlanBuilder Plan()
        {
            PlanBuilder planBuilder = new PlanBuilder();
            // this method cheats a bit by counting the action-pattern
            // and competences and also drive-collections to check when things are
            // allowed were.
            if (!Match(new string[] {"LPAREN",}))
                Error(string.Format("Plan needs to start with '(' rather than '{0}'" , token.value));
            NextToken();
            // action pattern, competence, docstring, drive collection
            int ap = 0, c = 0, d = 0, dc = 0;
            while (true)
            {
                if (!Match(new string[] {"LPAREN", "RPAREN"}))
                    Error(string.Format("Expected '(' as start of documentation / " +
                        "competence / action-pattern / drive-collection, or " +
                        "')' to end plan, instead of '{0}'" ,token.value));
                if (Match(new string[] {"RPAREN",}))
                {
                    // end of plan
                    NextToken();
                    break;
                }
                NextToken();
                // check for documentation
                if ( Match(new string[] {"DOC",}) )
                {
                    if ( ap + c + dc + d > 0 )
                        Error("Documentation only allowed as first " +
                            "element in plan");
                    d +=1;
                    planBuilder.setDocString(GetDocString());
                    // print docString();
                } 
                // check for competence
                else if (Match(new string[] {"C",}))
                {
                    c++;
                    planBuilder.addCompetence(GetCompetence());
                    // print competence()
                }
                // check for action-pattern
                else if ( Match(new string[] {"AP",}) )
                {
                    ap++;
                    planBuilder.addActionPattern(GetActionPattern());
                    // print actionPattern();
                }
                // check for drive-collection
                else if ( Match(new string[] {"DC", "RDC", "SDC", "SRDC"}) )
                {
                    if ( dc > 0 )
                        Error("Only a single drive-collection allowed");
                    dc ++;
                    planBuilder.SetDriveCollection(GetDriveCollection());
                    // print
                }
                else
                    Error(string.Format("Expected docstring / competence / action " +
                        "pattern or drive collection instead of '{0}'", token.value));
            }

            // the plan was closed
            if (token is Token)
                Error(string.Format("Illegal token '{0}' after end of plan" , token.value));
            if (dc == 0 && (ap+c) != 1)
                Error("Illegal plan: A plan without a drive-collection " +
                    "only allows for a SINLGE action-pattern OR a SINGLE competence");
           
            // everything is fine
            return planBuilder;
        }

        /// <summary>
        /// docstring ::= DOCUMENTATION COMMENT COMMENT COMMENT ")"
        /// </summary>
        /// <returns>The three comments in the form {string,string,string}.</returns>
        public string[] GetDocString()
        {
            if (!Match(new string[] {"DOC",}))
                Error(string.Format("Expected 'documentation' as start of docstring " +
                    "instead of '{0}'" ,token.value));
            NextToken();
            string[] docs = new string[3];
            for (int i = 0; i < 3; i++)
            {
                if (!Match(new string[] {"COMMENT",}))
                    Error(string.Format("Expected a comment of form \"...\" instead " +
                        "of '%s' in documentation", token.value));
                docs[i] =(token.value);
                NextToken();
            }
            if (!Match(new string[] {"RPAREN",}))
                Error(string.Format("Expected ')' to end docstring instead of '{0}'", token.value));
            NextToken();
            return docs;

        }

        /// <summary>
        /// drive-collection ::= <drive-collection-id> NAME
        ///                       ( NIL | "(" <goal> | )
        ///                       "(" DRIVES <drive-priorities> ")" ")"
        /// 
        /// If no goal is given, None is returned for the goal.
        /// </summary>
        /// <returns>The drive collection as 
        ///    (id, name, goal, [priority1, priority2, ...])</returns>
        public Tuple<string,string,List<object>,List<Tuple<string, List<object>, string, long>[]>> GetDriveCollection()
        {
            string cid = GetDriveCollectionId();
            List<object> goal = null;
            List<Tuple<string, List<object>, string, long>[]> priorities;

            if (!Match(new string[] {"NAME",}))
                Error(string.Format("Expected a valid drive collection name instead " +
                    "of '{0}'", token.value));
            string name = token.value;
            NextToken();
            // check if there is a goal and set it if given
            // ( NIL | "(" <goal> | ) "("

            if (Match(new string[] {"NIL",}))
            {
                // NIL "("
                NextToken();
                if (!Match(new string[] {"LPAREN",}))
                    Error(string.Format("Expected '(' after 'nil' instead of '{0}' in " +
                        "drive collection '{1}'" ,token.value, name));
                NextToken();

            }
            else
            {
                // "(" [ <goal> "(" ]
                if (!Match(new string[] {"LPAREN",}))
                    Error(string.Format("Expected '(' after drive collection name " +
                        "instead of '{0}' in drive collection '{1}'",token.value, name));
                NextToken();
                // check if a goal is specified
                if (Match(new string[] {"GOAL",}))
                {
                    // <goal> "("
                    goal = GetGoal();
                    if (!Match(new string[] {"LPAREN",}))
                        Error(string.Format("Expected '(' after goal " +
                            "instead of '{0}' in drive collection '{1}'",token.value, name));
                    NextToken();
                }
            }
            // get the drive priorities
            if (!Match(new string[] {"DRIVES",}))
                Error(string.Format("Expected 'drives' instead of '{0}' in drive " +
                    "collection '{1}'",token.value, name));
            NextToken();
            priorities = GetDrivePriorities();
            for(int i = 0; i < 2;i++)
            {
                if (!Match(new string[] {"RPAREN",}))
                    Error(string.Format("Expected ')' to end drive collection instead " +
                        "of '{0}' in drive collection '{1}'",token.value, name));
                NextToken();
            }

            return new Tuple<string,string,List<object>,List<Tuple<string, List<object>, string, long>[]>>(cid,name,goal,priorities);
        }

        /// <summary>
        /// drive-collection-id ::= DC | RDC | SDC | SRDC
        /// </summary>
        /// <returns>The drive collection id as a string.</returns>
        public string GetDriveCollectionId()
        {
            if (!Match(new string[] {"DC", "RDC", "SDC", "SRDC"}) )
                Error(string.Format("Expected the drive collection type instead of {0}'", token.value));
            string cid = token.token;
            NextToken();

            return cid;
        }

        /// <summary>
        /// drive_priorities ::= <drive-elements>+
        /// </summary>
        /// <returns>A list of drive priorities as given by driveElements().</returns>
        public List<Tuple<string, List<object>, string, long>[]> GetDrivePriorities()
        {
            List<Tuple<string, List<object>, string, long>[]> priorities;

            if (!Match(new string[] {"LPAREN",}))
                Error(string.Format("Expected '(' that starts list of drive elements " +
                    "instead of '{0}'" ,token.value));
            priorities = new List<Tuple<string, List<object>, string, long>[]>();
            while(Match(new string[] {"LPAREN",}))
                priorities.Add(GetDriveElements());

            return priorities;
        }

        /// <summary>
        /// drive-elements ::= "(" <drive-element>+ ")"
        /// </summary>
        /// <returns>A sequence of drive elements as given by driveElement</returns>
        public Tuple<string, List<object>, string, long>[] GetDriveElements()
        {
            List<Tuple<string, List<object>, string, long>> elements;
            if (!Match(new string[] {"LPAREN",}))
                Error(string.Format("Expected '(' that starts list of drive elements " +
                    "instead of '{0}'" ,token.value));
            NextToken();
            if (!Match(new string[] {"LPAREN",}))
                Error(string.Format("Expected '(' that starts list of drive elements " +
                    "instead of '{0}'" ,token.value));
            elements = new List<Tuple<string,List<object>,string,long>>();
            while (Match(new string[] {"LPAREN",}))
                elements.Add(GetDriveElement());
            if (!Match(new string[] {"RPAREN",}))
                Error(string.Format("Expected ')' to end list of drive elements " +
                    "instead of '{0}'" ,token.value));
            NextToken();

            return elements.ToArray();
        }

        /// <summary>
        /// drive-element ::= "(" NAME ( NIL | "(" <trigger> | ) NAME
        ///                         ( NIL | "(" <freq> | ) <opt-comment> ")"
        /// 
        /// If no trigger is given, then None is returned for the trigger.
        /// If no frequency is given, then 0 is returned for the frequency.
        /// </summary>
        /// <returns>The drive element as (name, trigger, triggerable, freq): Tuple(string, trigger, string, long).</returns>
        public Tuple<string, List<object>, string, long> GetDriveElement()
        {
            string name;
            List<object> trigger = null;
            string triggerable = null;
            long freq = 0;

            if (!Match(new string[] {"LPAREN",}))
                Error(string.Format("Expected '(' to start drive element instead " +
                    "of '{0}'", token.value));
            NextToken();
            if(!Match(new string[] {"NAME",}))
                Error(string.Format("Expected valid drive element name instead of '{0}'", 
                    token.value));
            name = token.value;
            NextToken();
            // ( NIL | "(" <trigger> | ) NAME
            if (!Match(new string[] {"NAME","LPAREN","NIL"}))
                Error(string.Format("Expected name of triggerable, '(' or 'nil' " +
                    "instead of '{0}' in drive element '{1}'", token.value, name));
            // get trigger if there is one
            if (Match(new string[] {"NIL","LPAREN"}))
            {
                if (Match(new string[] {"NIL",}))
                    NextToken();
                else
                {
                    NextToken();
                    trigger = GetTrigger();
                }
                if (!Match(new string[] {"NAME",}))
                    Error(string.Format("Expected name of triggerable instead of '%s' " +
                        "in drive elements '{0}'", token.value, name));     
            }
            // get triggerable (NAME)
            triggerable = token.value;
            NextToken();
            // check for frequency
            // ( NIL | "(" <freq> | )
            if (Match(new string[] {"LPAREN","NIL"}))
                if (Match(new string[] {"NIL",}))
                    NextToken();
                else
                {
                    NextToken();
                    freq = GetFreq();
                }
            // <opt-comment> ")"
            GetOptComment();
            if (!Match(new string[] {"RPAREN",}))
                Error(string.Format("Expected ')' instead of '%s' as the end of drive " +
                    "element '{0}'", token.value, name));
            NextToken();

            return new Tuple<string,List<object>,string,long>(name,trigger,triggerable,freq);
        }

        /// <summary>
        /// competence ::= C NAME ( NIL | "(" <time> | )
        ///                ( NIL | "(" <goal | ) "(" ELEMENTS
        ///                 <competence-priorities> ")" <opt-comment> ")"
        /// 
        /// If no time is given, them time is set to None.
        /// If no goal is given, the goal is set to None.
        /// </summary>
        /// <returns>The competence as (name, time, goal, priorities)
        ///     : (string, time, goal, priorities)</returns>
        public Tuple<string,long,List<object>,List<Tuple<string,List<object>,string,int> []>> GetCompetence()
        {
            string name;
            long time = 0;
            List<object> goal = null;
            List<Tuple<string,List<object>,string,int> []> priorities;

            // C NAME
            if (!Match(new string[] {"C",}))
                Error(string.Format("Expected 'C' as start of competence instead " +
                    "of '{0}'", token.value));
            NextToken();
            if (!Match(new string[] {"NAME",}))
                Error(string.Format("Expected valid competence name instead " +
                    "of '{0}'", token.value));
            name = token.value;
            NextToken();
            // ( NIL | "(" <time> | ) ( NIL | "(" <goal> | ) "("
            // The branching below should be checked (might have missed a case)
            if (!Match(new string[] {"LPAREN","NIL"}))
                Error(string.Format("Expected '(' or 'nil' after competence name " +
                    "instead of '{0}' in competence '{1}'", token.value,name));
            if (Match(new string[] {"NIL",}))
            {
                // NIL ( NIL | "(" <goal> | ) "("
                NextToken();
                if (!Match(new string[] {"LPAREN","NIL"}))
                    Error(string.Format("Expected '(' or 'nil' after 'nil' for time " +
                        "instead of '{0}' in competence '{1}'", token.value,name));
                if (Match(new string[] {"NIL",}))
                {
                    // NIL NIL "("
                    NextToken();
                    if (!Match(new string[] {"LPAREN",}))
                        Error(string.Format("Expected '(' after 'nil' for goal instead " +
                            "instead of '{0}' in competence '{1}'",token.value,name));
                    NextToken();
                }
                else
                {
                    // NIL "(" [ <goal> "(" ]
                    NextToken();
                    if (Match(new string[] {"GOAL",}))
                    {
                        goal = GetGoal();
                        if (!Match(new string[] {"LPAREN",}))
                            Error(string.Format("Expected '(' after goal instead of " +
                                "instead of '{0}' in competence '{1}'",token.value,name));
                        NextToken();
                    }
                }
            }
            else
            {
                // "(" ( <time> ( NIL | "(" <goal> | ) "(" | <goal> "(" | )
                NextToken();
                if (Match(new string[] {"HOURS","MINUTES","SECONDS","NONE"}))
                {
                    // "(" <time> ( NIL | "(" <goal> | ) "("
                    time = GetTime();
                    if (!Match(new string[] {"LPAREN","NIL"}))
                            Error(string.Format("Expected '(' or 'nil' after time instead " +
                                "instead of '{0}' in competence '{1}'",token.value,name));
                    if (Match(new string[] {"NIL",}))
                    {
                        // "(" <time> NIL "("
                        NextToken();
                        if (!Match(new string[] {"LPAREN",}))
                            Error(string.Format("Expected '(' after 'nil' for goal " +
                                "instead of '{0}' in competence '{1}'",token.value,name));
                        NextToken();
                    }
                    else
                    {
                        //  "(" <time> "(" [ <goal> "(" ]
                        NextToken();
                        if (Match(new string[] {"GOAL",}))
                        {
                            goal = GetGoal();
                            if (!Match(new string[] {"LPAREN",}))
                                Error(string.Format("Expected '(' after goal " +
                                    "instead of '{0}' in competence '{1}'",token.value,name));
                            NextToken();
                        }
                    }
                } 
                else if (Match(new string[] {"GOAL",}))
                {
                    //  "(" <goal> "("
                    goal = GetGoal();
                    if (!Match(new string[] {"LPAREN",}))
                        Error(string.Format("Expected '(' after goal " +
                            "instead of '{0}' in competence '{1}'",token.value,name));
                    NextToken();
                }
            }
            // competence priorities
            // ELEMENTS <competence-priorities> <opt-comment> ")"
            if (!Match(new string[] {"ELEMENTS",}))
                Error(string.Format("Expected 'elements' as start of element " +
                    "instead of '{0}' in competence '{1}'",token.value,name));
            NextToken();
            priorities = GetCompetencePriorities();
            if (!Match(new string[] {"RPAREN",}))
                Error(string.Format("Expected ')' to end competence " +
                    "instead of '{0}' in competence '{1}'",token.value,name));
            NextToken();
            GetOptComment();
            if (!Match(new string[] { "RPAREN", }))
                Error(string.Format("Expected ')' to end competence " +
                    "instead of '{0}' in competence '{1}'", token.value, name));
            NextToken();
            
            return new Tuple<string,long,List<object>,List<Tuple<string,List<object>,string,int> []>>(name,time,goal,priorities);
        }

        /// <summary>
        /// <code>
        /// <![CDATA[competence-priorities ::= <competence-elements>+]]>
        /// </code>
        /// </summary>
        /// <returns>A list of competence priorities.</returns>
        public List<Tuple<string,List<object>,string,int> []> GetCompetencePriorities()
        {
            List<Tuple<string,List<object>,string,int> []> priorities = new List<Tuple<string,List<object>,string,int>[]>();
            if (!Match(new string[] {"LPAREN",}))
                Error(string.Format("Expected '(' as start of a list of competence elements "+
                    "instead of '{0}'",token.value));
            while(Match(new string[] {"LPAREN",}))
                priorities.Add(GetCompetenceElements());

            return priorities;
        }
               
        /// <summary>
        /// <code>
        /// <![CDATA[competence-elements ::= "(" <competence-element>+ ")"]]>
        /// </code>
        /// </summary>
        /// <returns>A sequence of competence elements as given by
        ///     competence_element</returns>
        public Tuple<string,List<object>,string,int> [] GetCompetenceElements()
        {
            List<Tuple<string,List<object>,string,int>> elements = new List<Tuple<string,List<object>,string,int>>();

            if (!Match(new string[] {"LPAREN",}))
                Error(string.Format("Expected '(' as start of a list of competence elements "+
                    "instead of '{0}'",token.value));
            NextToken();
            // a competence element start with a '('
            if (!Match(new string[] {"LPAREN",}))
                Error(string.Format("Expected '(' as start a competence element "+
                    "instead of '{0}'",token.value));
            while (Match(new string[] {"LPAREN",}))
                elements.Add(GetCompetenceElement());
            if (!Match(new string[] {"RPAREN",}))
                Error(string.Format("Expected ')' as end of a list of competence elements "+
                    "instead of '{0}'",token.value));
            NextToken();

            return elements.ToArray();
        }
                
        /// <summary>
        /// <code>
        /// <![CDATA[
        /// competence-element ::= "(" NAME ( NIL | "(" <trigger> | ) NAME
        ///                             ( NIL | INTNUM | )
        ///                             <opt-comment> ")"
        /// ]]>
        /// </code>
        /// 
        /// If no number of retires is given, then 0 is returned.
        /// </summary>
        /// <returns>The competence element as 
        /// (name, trigger, triggerable, maxRetries)</returns>
        public Tuple<string,List<object>,string,int> GetCompetenceElement()
        {
            string name;
            List<object> trigger = null;
            string triggerable;
            int maxRetries;

            // "(" NAME
            if (!Match(new string[] {"LPAREN",}))
                Error(string.Format("Expected '(' to start a competence element "+
                    "instead of '{0}'",token.value));
            NextToken();
            if (!Match(new string[] {"NAME",}))
                Error(string.Format("Expected competence element name "+
                    "instead of '{0}'",token.value));
            name = token.value;
            NextToken();
            // check for trigger
            // ( NIL | "(" <trigger> | )
            if (Match(new string[] {"NIL",}))
                NextToken();
            else if (Match(new string[] {"LPAREN",}))
            {
                NextToken();
                trigger = GetTrigger();
            }
            // NAME
            if (!Match(new string[] {"NAME",}))
                Error(string.Format("Expected name of triggerable "+
                    "instead of '{0}' in competence '{1}'",token.value,name));
            triggerable = token.value;
            NextToken();
            // check for maxRetries
            // ( NIL | INTNUM | )
            maxRetries = 0;
            if (Match(new string[] {"NIL",}))
                NextToken();
            else if (Match(new string[] {"NUMINT",}))
            {
                maxRetries = int.Parse(token.value);
                NextToken();
            }
            // <opt-comment> ")"
            GetOptComment();
            if (!Match(new string[] {"RPAREN",}))
                Error(string.Format("Expected ')' to end competence element "+
                    "instead of '{0}' in competence '{1}'",token.value,name));
            NextToken();
            
            return new Tuple<string,List<object>,string,int>(name, trigger, triggerable, maxRetries);
        }
        
        /// <summary>
        /// <code>
        /// <![CDATA[
        /// aption-pattern ::= AP NAME ( NIL | "(" <time> | )
        ///                     "(" <action-pattern-elements> <opt-comment> ")"
        /// ]]>
        /// If no time is given, None is returned for the time.
        /// </code>
        /// </summary>
        /// <returns>The action pattern as (name, time, [name1, name2, ...])
        ///     (string, long, (string or full-sense (<code>Tuple[string,string,string]</code>), ...))
        /// </returns>
        public Tuple<string,long,List<object>> GetActionPattern()
        {
            string name;
            long time = 0;
            List<object> elements;

            // AP NAME
             if (!Match(new string[] {"AP",}))
                Error(string.Format("Expected 'AP' instead of '{0}'",token.value));
            NextToken();
            if (!Match(new string[] {"NAME",}))
                Error(string.Format("{0}' is not a valid name for an action pattern",token.value));
            name = token.value;
            NextToken();

            // ( NIL | "(" <time> | ) "("
            if (Match(new string[] {"NIL",}))
            {
                // NIL "("
                NextToken();
                if (!Match(new string[] {"LPAREN",}))
                    Error(string.Format("Expected '(' after 'nil' for time instead of '{0}'"+
                        "in action pattern '{1}'",token.value,name) );
                NextToken();
            }
            else if (Match(new string[] {"LPAREN",}))
            {
                // "(" [ <time> "(" ]
                NextToken();
                if (Match(new string[] {"HOURS","MINUTES","SECONDS","NONE"}))
                {
                    // "(" <time> "("
                    time = GetTime();
                    if (!Match(new string[] {"LPAREN",}))
                        Error(string.Format("Expected '(' after time instead of '{0}'"+
                            "in action pattern '{1}'",token.value,name) );
                    NextToken();
                }
            }
            else
                Error(string.Format("Expected '(' or 'nil' after action pattern name "+
                    "instead of '{0}' in action pattern '{1}'",token.value,name) );
            // proceed with action pattern element list
            // <action-pattern-elements> <opt-comment> ")"
            elements = GetActionPatternElements();
            GetOptComment();

            if (!Match(new string[] {"RPAREN",}))
                    Error(string.Format("Expected ')' instead of '{0}'"+
                        "in action pattern '{1}'",token.value,name) );
            NextToken();

            return new Tuple<string,long,List<object>>(name,time,elements);
        }

        /// <summary>
        /// <code>
        /// <![CDATA[action-pattern-elements ::= ( <full-sense> | NAME )+ ")"]]>
        /// </code>
        /// </summary>
        /// <returns>A list of action pattern elements given as senses 
        /// (<code>string</code>) and full-senses 
        /// (<code>Tuple[string,string,string]</code>).</returns>
        public List<object> GetActionPatternElements()
        {
            List<object> elements = new List<object>();

            if (!Match(new string[] {"LPAREN","NAME"}))
                Error(string.Format("Expected an action pattern element name of '(' "+
                    "instead of '{0}'",token.value));
            while (Match(new string[] {"NAME","LPAREN"}))
            {
                if(Match(new string[] {"LPAREN",}))
                    elements.Add(GetFullSenses());
                else
                {
                    elements.Add(token.value);
                    NextToken();
                }
            }
            if (!Match(new string[] {"RPAREN",}))
                Error(string.Format("Expected ')' to end action pattern instead of '{0}'",token.value));
            NextToken();

            return elements;
        }
        
        /// <summary>
        /// <code>
        /// <![CDATA[goal ::= GOAL <senses> ")"]]>
        /// </code>
        /// 
        /// If the list of senses is empty, then None is returned.
        /// </summary>
        /// <returns>>A list of senses (<code>string</code>) and full-senses 
        /// (<code>Tuple[string,string,string]</code>) that were given as the goal</returns>
        public List<object> GetGoal()
        {
            List<object> senses;

            if (!Match(new string[] {"GOAL",}))
                Error(string.Format("Expected 'goal' instead of '{0}'",token.value));
            NextToken();
            senses = GetSenses();
            if (!Match(new string[] {"RPAREN",}))
                Error(string.Format("Expected ')' as the end of a goal instead of '{0}'",token.value));
            NextToken();

            return (senses is List<object> && senses.Count > 0) ? senses : null;
        }

        /// <summary>
        /// <code>
        /// <![CDATA[trigger ::= TRIGGER <senses> ")"]]>
        /// </code>
        /// If the list of senses is empty, then None is returned.
        /// </summary>
        /// <returns>A list of senses (<code>string</code>) and full-senses 
        /// (<code>Tuple[string,string,string]</code>) that were given as the trigger</returns>
        public List<object> GetTrigger()
        {
            List<object> senses;

            if (!Match(new string[] {"TRIGGER",}))
                Error(string.Format("Expected 'trigger' instead of '{0}'",token.value));
            NextToken();
            senses = GetSenses();
            if (!Match(new string[] {"RPAREN",}))
                Error(string.Format("Expected ')' as the end of a trigger "+
                    "instead of '{0}'",token.value));
            NextToken();
            
            return (senses is List<object> && senses.Count > 0) ? senses : null;
        }

        /// <summary>
        /// <code>
        /// <![CDATA[
        /// senses ::= ( NIL | "(" ( NAME | <full-sense> )+ ")" )
        /// ]]>
        /// </code>
        /// If NIL is given, an empty list is returned.
        /// </summary>
        /// <returns>An object list containing senses (<code>string</code>) and full-senses 
        /// (<code>Tuple[string,string,string]</code>).</returns>
        public List<object> GetSenses()
        {
            List<object> elements;

            if (Match(new string[] {"NIL",}))
            {
                NextToken();
                return new List<object>();
            }
            if (!Match(new string[] {"LPAREN",}))
                Error(string.Format("Expected '(' instead of '{0}'",token.value));
            NextToken();
            elements = new List<object>();
            while (true)
            {
                if (Match(new string[] {"RPAREN",}))
                    break;
                if (!Match(new string[] {"LPAREN","NAME"}))
                    Error(string.Format("Expected either a sense-act name or '(' "+
                        "instead of '{0}'",token.value));
                // differentiate between sense-acts and senses
                if (Match(new string[] {"NAME",}))
                {
                    elements.Add(token.value);
                    NextToken();
                } 
                else
                    elements.Add(GetFullSenses());
            }
            // matches ')'
            NextToken();

            return elements;
        }

        /// <summary>
        /// <code>
        /// <![CDATA[full-sense ::= "(" NAME [<value> [<predicate>]] ")"]]>
        /// </code>
        /// </summary>
        /// <returns>The full sense, and None for the elements that
        ///     are not specified.</returns>
        public Tuple<string,string,string> GetFullSenses()
        {
            string name = null, value = null, pred = null;
            

            if (!Match(new string[] {"LPAREN",}))
                Error(string.Format("Expected '(' instead of '{0}'",token.value));
            NextToken();
            if (!Match(new string[] {"NAME",}))
                Error(string.Format("Expected sense name instead of '{0}'",token.value));
            name = token.value;
            NextToken();
            if (!Match(new string[] {"RPAREN",}))
            {
                value = GetValue();
                if (!Match(new string[] {"RPAREN",}))
                    pred = GetPredicate();
            }
            if (!Match(new string[] {"RPAREN",}))
                Error(string.Format("Expected ')' instead of '{0}'",token.value));
            NextToken();

            return new Tuple<string,string,string>(name,value,pred);
        }

        /// <summary>
        /// value ::= NUMINT | NUMFLOAT | NAME
        /// </summary>
        /// <returns>The value as string.</returns>
        public string GetValue()
        {
            if (!Match(new string[] {"NUMINT","NUMFLOAT","NAME","STRINGVALUE","NIL"}))
                Error(string.Format("Expected a valid sense value " +
                    " instead of '{0}'", token.value));
            string value = token.value;
            NextToken();

            return value;
        }

        /// <summary>
        /// predicate ::= PREDICATE
        /// </summary>
        /// <returns>The predicate as a string.</returns>
        public string GetPredicate()
        {
            if (!Match(new string[] {"PREDICATE",}))
                Error(string.Format("Expected a valid sense predicate " +
                    " instead of '{0}'", token.value));
            string pred = token.value;
            NextToken();

            return pred;
        }

        /// <summary>
        /// freq ::= <freq-unit> <numfloat> ")"
        /// </summary>
        /// <returns>frequency as period time</returns>
        public long GetFreq()
        {
            string unit = GetFreqUnit();
            float value = GetNumFloat();
            if (!Match(new string[] {"RPAREN",}))
                Error(string.Format("Expected ')' instead of '{0}'",token.value));
            NextToken();
            //process the frequency unit
            switch (unit)
            {
                case "HOURS":
                    return (long) (3600000.0 * value);
                case "MINUTES":
                    return (long) (60000.0 * value);
                case "SECONDS":
                    return (long) (1000.0 * value);
                case "HZ":
                    return (long) (1000.0 / value);
                case "PM":
                    return (long) (60000.0 / value);                
                default:
                    return (long) value;
            }
        }

        /// <summary>
        /// freq-unit ::= HOURS | MINUTES | SECONDS | HZ | PM | NONE
        /// </summary>
        /// <returns>The token string of the frequency unit.</returns>
        public string GetFreqUnit()
        {
            if (!Match(new string[] {"HOURS", "MINUTES", "SECONDS", "HZ", "PM", "NONE"}))
                Error(string.Format("Expected a valid frequency unit " +
                    "instead of '{0}'",token.value));
            string unit = token.token;
            NextToken();

            return unit;
        }

        /// <summary>
        /// time ::= <time-unit> <numfloat> ")"
        /// </summary>
        /// <returns>time in milliseconds</returns>
        public long GetTime()
        {
            string unit = GetTimeUnit();
            float value = GetNumFloat();

            if (!Match(new string[] {"RPAREN",}))
                Error(string.Format("Expected ')' instead of '{0}'",token.value));
            NextToken();
            // process the time unit
            switch (unit)
            {
                case "HOURS":
                    return (long) (3600000.0 * value);
                case "MINUTES":
                    return (long) (60000.0 * value);
                case "SECONDS":
                    return (long) (1000.0 * value);
                default:
                    return (long) value;
            }
        }

        /// <summary>
        /// translates the internal time representation from long back to string
        /// </summary>
        /// <returns>timeas string including the unit</returns>
        public static string GetTimeString(long time)
        {
            

            // process the time unit
            if (time <= 0)
                return "";
            if (time < 0.001)
                return "(seconds 0.001)";
            if (time < 60000)
                return String.Format("(seconds {0:G29})",time/1000);
            if (time < 3600000)
                return String.Format("(minutes {0:G29})",time/60000);
            
            return String.Format("(hours {0:G29})",time/3600000);
        }



        /// <summary>
        /// time-unit ::= HOURS | MINUTES | SECONDS | NONE
        /// </summary>
        /// <returns>The unit as token string.</returns>
        public string GetTimeUnit()
        {
            if (!Match(new string[] {"HOURS", "MINUTES","SECONDS","NONE"}))
                Error(string.Format("Expected a valid time unit " +
                    "instead of '{0}'",token.value));
            string unit = token.value;
            NextToken();

            return unit;
        }

        /// <summary>
        /// numfloat ::= NUMINT | NUMFLOAT
        /// </summary>
        /// <returns>The number as float.</returns>
        public float GetNumFloat()
        {
            if (!Match(new string[] {"NUMINT", "NUMFLOAT"}))
                Error(string.Format("Expected a floating-point number " +
                    "instead of '{0}'",token.value));
            string value = token.value;
            NextToken();

            return float.Parse(value);
        }

        /// <summary>
        /// opt-comment ::= COMMENT |
        /// </summary>
        public void GetOptComment()
        {
            if (Match(new string[] {"COMMENT",}))
                NextToken();
        }
    }
}
//# test lexer on file given as only argument
//#f = open(sys.argv[1]).read()
//#l = LAPLexer(f)
//#while True:
//#    t = l.token()
//#    if t:
//#        print "%s: '%s'" % (t.token, t.value)
//#    else:
//#        break


//# entering plans at the command line
//# while 1:
//#    try:
//#        s = raw_input()
//#    except EOFError:
//#        break
//#    try:
//#        LAPParser(s)
//#    except ParseError, msg:
//#        print msg

//# test parser on file given as only argument
//#f = open(sys.argv[1]).read()
//#try:
//#    p = LAPParser()
//#    p.parse(f)
//#except ParseError, msg:
//#    print "ParseError:", msg
