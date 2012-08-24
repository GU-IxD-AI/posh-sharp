using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys.exceptions;
using POSH_sharp.sys.strict;



 namespace POSH_sharp.sys.parse
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
    class LAPParser
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

        private LAPLexer lex;
        private Token token;

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
        public PlanBuilder parse( string inputString)
        {
            lex = new LAPLexer(inputString);
            return start();
        }

        /// <summary>
        /// Gets the next token from the lexer.
        /// </summary>
        public void nextToken()
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
        public bool match(string[] allowedTokens)
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
        public void error(string msg)
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
        public PlanBuilder start()
        {
            nextToken();
            return plan();
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
        PlanBuilder plan()
        {
            PlanBuilder planBuilder = new PlanBuilder();
            // this method cheats a bit by counting the action-pattern
            // and competences and also drive-collections to check when things are
            // allowed were.
            if (!match(new string[] {"LPAREN",}))
                error(string.Format("Plan needs to start with '(' rather than '{0}'" , token.value));
            nextToken();
            // action pattern, competence, docstring, drive collection
            int ap = 0, c = 0, d = 0, dc = 0;
            while (true)
            {
                if (!match(new string[] {"LPAREN", "RPAREN"}))
                    error(string.Format("Expected '(' as start of documentation / " +
                        "competence / action-pattern / drive-collection, or " +
                        "')' to end plan, instead of '{0}'" ,token.value));
                if (match(new string[] {"RPAREN",}))
                {
                    // end of plan
                    nextToken();
                    break;
                }
                nextToken();
                // check for documentation
                if ( match(new string[] {"DOCUMENTATION",}) )
                {
                    if ( ap + c + dc + d > 0 )
                        error("Documentation only allowed as first " +
                            "element in plan");
                    d +=1;
                    planBuilder.setDocString(getDocString());
                    // print docString();
                } 
                // check for competence
                else if (match(new string[] {"C",}))
                {
                    c++;
                    planBuilder.addCompetence(getCompetence());
                    // print competence()
                }
                // check for action-pattern
                else if ( match(new string[] {"AP",}) )
                {
                    ap++;
                    planBuilder.addActionPattern(getActionPattern());
                    // print actionPattern();
                }
                // check for drive-collection
                else if ( match(new string[] {"DC", "RDC", "SDC", "SRDC"}) )
                {
                    if ( dc > 0 )
                        error("Only a single drive-collection allowed");
                    dc ++;
                    planBuilder.setDriveCollection(getDriveCollection());
                    // print
                }
                else
                    error(string.Format("Expected docstring / competence / action " +
                        "pattern or drive collection instead of '{0}'", token.value));
            }

            // the plan was closed
            if (token is Token)
                error(string.Format("Illegal token '{0}' after end of plan" , token.value));
            if (dc == 0 && (ap+c) != 1)
                error("Illegal plan: A plan without a drive-collection " +
                    "only allows for a SINLGE action-pattern OR a SINGLE competence");
           
            // everything is fine
            return planBuilder;
        }

        /// <summary>
        /// docstring ::= DOCUMENTATION COMMENT COMMENT COMMENT ")"
        /// </summary>
        /// <returns>The three comments in the form {string,string,string}.</returns>
        public string[] getDocString()
        {
            if (!match(new string[] {"DOCUMENTATION",}))
                error(string.Format("Expected 'documentation' as start of docstring " +
                    "instead of '{0}'" ,token.value));
            nextToken();
            string[] docs = new string[3];
            for (int i = 0; i < 3; i++)
            {
                if (!match(new string[] {"COMMENT",}))
                    error(string.Format("Expected a comment of form \"...\" instead " +
                        "of '%s' in documentation", token.value));
                docs[i] =(token.value.Substring(1,token.value.Length-2));
                nextToken();
            }
            if (!match(new string[] {"RPAREN",}))
                error(string.Format("Expected ')' to end docstring instead of '{0}'", token.value));
            nextToken();
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
        public Tuple<string,string,List<object>,List<Tuple<string, List<object>, string, long>[]>> getDriveCollection()
        {
            string cid = getDriveCollectionId();
            List<object> goal = null;
            List<Tuple<string, List<object>, string, long>[]> priorities;

            if (!match(new string[] {"NAME",}))
                error(string.Format("Expected a valid drive collection name instead " +
                    "of '{0}'", token.value));
            string name = token.value;
            nextToken();
            // check if there is a goal and set it if given
            // ( NIL | "(" <goal> | ) "("

            if (match(new string[] {"NIL",}))
            {
                // NIL "("
                nextToken();
                if (!match(new string[] {"LPAREN",}))
                    error(string.Format("Expected '(' after 'nil' instead of '{0}' in " +
                        "drive collection '{1}'" ,token.value, name));
                nextToken();

            }
            else
            {
                // "(" [ <goal> "(" ]
                if (!match(new string[] {"LPAREN",}))
                    error(string.Format("Expected '(' after drive collection name " +
                        "instead of '{0}' in drive collection '{1}'",token.value, name));
                nextToken();
                // check if a goal is specified
                if (match(new string[] {"GOAL",}))
                {
                    // <goal> "("
                    goal = getGoal();
                    if (!match(new string[] {"LPAREN",}))
                        error(string.Format("Expected '(' after goal " +
                            "instead of '{0}' in drive collection '{1}'",token.value, name));
                    nextToken();
                }
            }
            // get the drive priorities
            if (!match(new string[] {"DRIVES",}))
                error(string.Format("Expected 'drives' instead of '{0}' in drive " +
                    "collection '{1}'",token.value, name));
            nextToken();
            priorities = getDrivePriorities();
            for(int i = 0; i < 2;i++)
            {
                if (!match(new string[] {"RPAREN",}))
                    error(string.Format("Expected ')' to end drive collection instead " +
                        "of '{0}' in drive collection '{1}'",token.value, name));
                nextToken();
            }

            return new Tuple<string,string,List<object>,List<Tuple<string, List<object>, string, long>[]>>(cid,name,goal,priorities);
        }

        /// <summary>
        /// drive-collection-id ::= DC | RDC | SDC | SRDC
        /// </summary>
        /// <returns>The drive collection id as a string.</returns>
        public string getDriveCollectionId()
        {
            if (!match(new string[] {"DC", "RDC", "SDC", "SRDC"}) )
                error(string.Format("Expected the drive collection type instead of {0}'", token.value));
            string cid = token.token;
            nextToken();

            return cid;
        }

        /// <summary>
        /// drive_priorities ::= <drive-elements>+
        /// </summary>
        /// <returns>A list of drive priorities as given by driveElements().</returns>
        public List<Tuple<string, List<object>, string, long>[]> getDrivePriorities()
        {
            List<Tuple<string, List<object>, string, long>[]> priorities;

            if (!match(new string[] {"LPAREN",}))
                error(string.Format("Expected '(' that starts list of drive elements " +
                    "instead of '{0}'" ,token.value));
            priorities = new List<Tuple<string, List<object>, string, long>[]>();
            while(match(new string[] {"LPAREN",}))
                priorities.Add(getDriveElements());

            return priorities;
        }

        /// <summary>
        /// drive-elements ::= "(" <drive-element>+ ")"
        /// </summary>
        /// <returns>A sequence of drive elements as given by driveElement</returns>
        public Tuple<string, List<object>, string, long>[] getDriveElements()
        {
            List<Tuple<string, List<object>, string, long>> elements;
            if (!match(new string[] {"LPAREN",}))
                error(string.Format("Expected '(' that starts list of drive elements " +
                    "instead of '{0}'" ,token.value));
            nextToken();
            if (!match(new string[] {"LPAREN",}))
                error(string.Format("Expected '(' that starts list of drive elements " +
                    "instead of '{0}'" ,token.value));
            elements = new List<Tuple<string,List<object>,string,long>>();
            while (match(new string[] {"LPAREN",}))
                elements.Add(getDriveElement());
            if (!match(new string[] {"RPAREN",}))
                error(string.Format("Expected ')' to end list of drive elements " +
                    "instead of '{0}'" ,token.value));
            nextToken();

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
        public Tuple<string, List<object>, string, long> getDriveElement()
        {
            string name;
            List<object> trigger = null;
            string triggerable = null;
            long freq = 0;

            if (!match(new string[] {"LPAREN",}))
                error(string.Format("Expected '(' to start drive element instead " +
                    "of '{0}'", token.value));
            nextToken();
            if(!match(new string[] {"NAME",}))
                error(string.Format("Expected valid drive element name instead of '{0}'", 
                    token.value));
            name = token.value;
            nextToken();
            // ( NIL | "(" <trigger> | ) NAME
            if (!match(new string[] {"NAME","LPAREN","NIL"}))
                error(string.Format("Expected name of triggerable, '(' or 'nil' " +
                    "instead of '{0}' in drive element '{1}'", token.value, name));
            // get trigger if there is one
            if (match(new string[] {"NIL","LPAREN"}))
            {
                if (match(new string[] {"NIL",}))
                    nextToken();
                else
                {
                    nextToken();
                    trigger = getTrigger();
                }
                if (!match(new string[] {"NAME",}))
                    error(string.Format("Expected name of triggerable instead of '%s' " +
                        "in drive elements '{0}'", token.value, name));     
            }
            // get triggerable (NAME)
            triggerable = token.value;
            nextToken();
            // check for frequency
            // ( NIL | "(" <freq> | )
            if (match(new string[] {"LPAREN","NIL"}))
                if (match(new string[] {"NIL",}))
                    nextToken();
                else
                {
                    nextToken();
                    freq = getFreq();
                }
            // <opt-comment> ")"
            getOptComment();
            if (!match(new string[] {"RPAREN",}))
                error(string.Format("Expected ')' instead of '%s' as the end of drive " +
                    "element '{0}'", token.value, name));
            nextToken();

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
        public Tuple<string,long,List<object>,List<Tuple<string,List<object>,string,int> []>> getCompetence()
        {
            string name;
            long time = 0;
            List<object> goal = null;
            List<Tuple<string,List<object>,string,int> []> priorities;

            // C NAME
            if (!match(new string[] {"C",}))
                error(string.Format("Expected 'C' as start of competence instead " +
                    "of '{0}'", token.value));
            nextToken();
                        if (!match(new string[] {"NAME",}))
                error(string.Format("Expected valid competence name instead " +
                    "of '{0}'", token.value));
            name = token.value;
            nextToken();
            // ( NIL | "(" <time> | ) ( NIL | "(" <goal> | ) "("
            // The branching below should be checked (might have missed a case)
            if (!match(new string[] {"LPAREN","NIL"}))
                error(string.Format("Expected '(' or 'nil' after competence name " +
                    "instead of '{0}' in competence '{1}'", token.value,name));
            if (!match(new string[] {"NIL",}))
            {
                // NIL ( NIL | "(" <goal> | ) "("
                nextToken();
                if (!match(new string[] {"LPAREN","NIL"}))
                    error(string.Format("Expected '(' or 'nil' after 'nil' for time " +
                        "instead of '{0}' in competence '{1}'", token.value,name));
                if (match(new string[] {"NIL",}))
                {
                    // NIL NIL "("
                    nextToken();
                    if (!match(new string[] {"LPAREN",}))
                        error(string.Format("Expected '(' after 'nil' for goal instead " +
                            "instead of '{0}' in competence '{1}'",token.value,name));
                    nextToken();
                }
                else
                {
                    // NIL "(" [ <goal> "(" ]
                    nextToken();
                    if (match(new string[] {"GOAL",}))
                    {
                        goal = getGoal();
                        if (!match(new string[] {"LPAREN",}))
                            error(string.Format("Expected '(' after goal instead of " +
                                "instead of '{0}' in competence '{1}'",token.value,name));
                        nextToken();
                    }
                }
            }
            else
            {
                // "(" ( <time> ( NIL | "(" <goal> | ) "(" | <goal> "(" | )
                nextToken();
                if (match(new string[] {"HOURS","MINUTES","SECONDS","NONE"}))
                {
                    // "(" <time> ( NIL | "(" <goal> | ) "("
                    time = getTime();
                    if (!match(new string[] {"LPAREN","NIL"}))
                            error(string.Format("Expected '(' or 'nil' after time instead " +
                                "instead of '{0}' in competence '{1}'",token.value,name));
                    if (match(new string[] {"NIL",}))
                    {
                        // "(" <time> NIL "("
                        nextToken();
                        if (!match(new string[] {"LPAREN",}))
                            error(string.Format("Expected '(' after 'nil' for goal " +
                                "instead of '{0}' in competence '{1}'",token.value,name));
                        nextToken();
                    }
                    else
                    {
                        //  "(" <time> "(" [ <goal> "(" ]
                        nextToken();
                        if (match(new string[] {"GOAL",}))
                        {
                            goal = getGoal();
                            if (!match(new string[] {"LPAREN",}))
                                error(string.Format("Expected '(' after goal " +
                                    "instead of '{0}' in competence '{1}'",token.value,name));
                            nextToken();
                        }
                    }
                } 
                else if (match(new string[] {"GOAL",}))
                {
                    //  "(" <goal> "("
                    goal = getGoal();
                    if (!match(new string[] {"LPAREN",}))
                        error(string.Format("Expected '(' after goal " +
                            "instead of '{0}' in competence '{1}'",token.value,name));
                    nextToken();
                }
            }
            // competence priorities
            // ELEMENTS <competence-priorities> <opt-comment> ")"
            if (!match(new string[] {"ELEMENTS",}))
                error(string.Format("Expected 'elements' as start of element " +
                    "instead of '{0}' in competence '{1}'",token.value,name));
            nextToken();
            priorities = getCompetencePriorities();
            if (!match(new string[] {"RPAREN",}))
                error(string.Format("Expected ')' to end competence " +
                    "instead of '{0}' in competence '{1}'",token.value,name));
            nextToken();

            return new Tuple<string,long,List<object>,List<Tuple<string,List<object>,string,int> []>>(name,time,goal,priorities);
        }

        /// <summary>
        /// <code>
        /// <![CDATA[competence-priorities ::= <competence-elements>+]]>
        /// </code>
        /// </summary>
        /// <returns>A list of competence priorities.</returns>
        public List<Tuple<string,List<object>,string,int> []> getCompetencePriorities()
        {
            List<Tuple<string,List<object>,string,int> []> priorities = new List<Tuple<string,List<object>,string,int>[]>();
            if (!match(new string[] {"LPAREN",}))
                error(string.Format("Expected '(' as start of a list of competence elements "+
                    "instead of '{0}'",token.value));
            while(match(new string[] {"LPAREN",}))
                priorities.Add(getCompetenceElements());

            return priorities;
        }
               
        /// <summary>
        /// <code>
        /// <![CDATA[competence-elements ::= "(" <competence-element>+ ")"]]>
        /// </code>
        /// </summary>
        /// <returns>A sequence of competence elements as given by
        ///     competence_element</returns>
        public Tuple<string,List<object>,string,int> [] getCompetenceElements()
        {
            List<Tuple<string,List<object>,string,int>> elements = new List<Tuple<string,List<object>,string,int>>();

            if (!match(new string[] {"LPAREN",}))
                error(string.Format("Expected '(' as start of a list of competence elements "+
                    "instead of '{0}'",token.value));
            nextToken();
            // a competence element start with a '('
            if (!match(new string[] {"LPAREN",}))
                error(string.Format("Expected '(' as start a competence element "+
                    "instead of '{0}'",token.value));
            while (match(new string[] {"LPAREN",}))
                elements.Add(getCompetenceElement());
            if (!match(new string[] {"RPAREN",}))
                error(string.Format("Expected ')' as end of a list of competence elements "+
                    "instead of '{0}'",token.value));
            nextToken();

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
        /// If no number of retires is given, then -1 is returned.
        /// </summary>
        /// <returns>The competence element as 
        /// (name, trigger, triggerable, maxRetries)</returns>
        public Tuple<string,List<object>,string,int> getCompetenceElement()
        {
            string name;
            List<object> trigger = null;
            string triggerable;
            int maxRetries;

            // "(" NAME
            if (!match(new string[] {"LPAREN",}))
                error(string.Format("Expected '(' to start a competence element "+
                    "instead of '{0}'",token.value));
            nextToken();
            if (!match(new string[] {"NAME",}))
                error(string.Format("Expected competence element name "+
                    "instead of '{0}'",token.value));
            name = token.value;
            nextToken();
            // check for trigger
            // ( NIL | "(" <trigger> | )
            if (match(new string[] {"NIL",}))
                nextToken();
            else if (match(new string[] {"LPAREN",}))
            {
                nextToken();
                trigger = getTrigger();
            }
            // NAME
            if (!match(new string[] {"NAME",}))
                error(string.Format("Expected name of triggerable "+
                    "instead of '{0}' in competence '{1}'",token.value,name));
            triggerable = token.value;
            nextToken();
            // check for maxRetries
            // ( NIL | INTNUM | )
            maxRetries = -1;
            if (match(new string[] {"NIL",}))
                nextToken();
            else if (match(new string[] {"NUMINT",}))
            {
                maxRetries = int.Parse(token.value);
                nextToken();
            }
            // <opt-comment> ")"
            getOptComment();
            if (!match(new string[] {"RPAREN",}))
                error(string.Format("Expected ')' to end competence element "+
                    "instead of '{0}' in competence '{1}'",token.value,name));
            nextToken();
            
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
        public Tuple<string,long,List<object>> getActionPattern()
        {
            string name;
            long time = 0;
            List<object> elements;

            // AP NAME
             if (!match(new string[] {"AP",}))
                error(string.Format("Expected 'AP' instead of '{0}'",token.value));
            nextToken();
            if (!match(new string[] {"NAME",}))
                error(string.Format("{0}' is not a valid name for an action pattern",token.value));
            name = token.value;
            nextToken();

            // ( NIL | "(" <time> | ) "("
            if (match(new string[] {"NIL",}))
            {
                // NIL "("
                nextToken();
                if (!match(new string[] {"LPAREN",}))
                    error(string.Format("Expected '(' after 'nil' for time instead of '{0}'"+
                        "in action pattern '{1}'",token.value,name) );
                nextToken();
            }
            else if (match(new string[] {"LPAREN",}))
            {
                // "(" [ <time> "(" ]
                nextToken();
                if (match(new string[] {"HOURS","MINUTES","SECONDS","NONE"}))
                {
                    // "(" <time> "("
                    time = getTime();
                    if (!match(new string[] {"LPAREN",}))
                        error(string.Format("Expected '(' after time instead of '{0}'"+
                            "in action pattern '{1}'",token.value,name) );
                    nextToken();
                }
            }
            else
                error(string.Format("Expected '(' or 'nil' after action pattern name "+
                    "instead of '{0}' in action pattern '{1}'",token.value,name) );
            // proceed with action pattern element list
            // <action-pattern-elements> <opt-comment> ")"
            elements = getActionPatternElements();
            getOptComment();

            if (!match(new string[] {"RPAREN",}))
                    error(string.Format("Expected ')' instead of '{0}'"+
                        "in action pattern '{1}'",token.value,name) );
            nextToken();

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
        public List<object> getActionPatternElements()
        {
            List<object> elements = new List<object>();

            if (!match(new string[] {"LPAREN","NAME"}))
                error(string.Format("Expected an action pattern element name of '(' "+
                    "instead of '{0}'",token.value));
            while (match(new string[] {"NAME","LPAREN"}))
            {
                if(match(new string[] {"LPAREN",}))
                    elements.Add(getFullSenses());
                else
                {
                    elements.Add(token.value);
                    nextToken();
                }
            }
            if (!match(new string[] {"RPAREN",}))
                error(string.Format("Expected ')' to end action pattern instead of '{0}'",token.value));
            nextToken();

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
        public List<object> getGoal()
        {
            List<object> senses;

            if (!match(new string[] {"GOAL",}))
                error(string.Format("Expected 'goal' instead of '{0}'",token.value));
            nextToken();
            senses = getSenses();
            if (!match(new string[] {"RPAREN",}))
                error(string.Format("Expected ')' as the end of a goal instead of '{0}'",token.value));
            nextToken();

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
        public List<object> getTrigger()
        {
            List<object> senses;

            if (!match(new string[] {"TRIGGER",}))
                error(string.Format("Expected 'trigger' instead of '{0}'",token.value));
            nextToken();
            senses = getSenses();
            if (!match(new string[] {"RPAREN",}))
                error(string.Format("Expected ')' as the end of a trigger "+
                    "instead of '{0}'",token.value));
            nextToken();
            
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
        public List<object> getSenses()
        {
            List<object> elements;

            if (match(new string[] {"NIL",}))
            {
                nextToken();
                return new List<object>();
            }
            if (!match(new string[] {"LPAREN",}))
                error(string.Format("Expected '(' instead of '{0}'",token.value));
            nextToken();
            elements = new List<object>();
            while (true)
            {
                if (match(new string[] {"RPAREN",}))
                    break;
                if (!match(new string[] {"LPAREN","NAME"}))
                    error(string.Format("Expected either a sense-act name or '(' "+
                        "instead of '{0}'",token.value));
                // differentiate between sense-acts and senses
                if (match(new string[] {"NAME",}))
                {
                    elements.Add(token.value);
                    nextToken();
                } 
                else
                    elements.Add(getFullSenses());
            }
            // matches ')'
            nextToken();

            return elements;
        }

        /// <summary>
        /// <code>
        /// <![CDATA[full-sense ::= "(" NAME [<value> [<predicate>]] ")"]]>
        /// </code>
        /// </summary>
        /// <returns>The full sense, and None for the elements that
        ///     are not specified.</returns>
        public Tuple<string,string,string> getFullSenses()
        {
            string name = null, value = null, pred = null;
            

            if (!match(new string[] {"LPAREN",}))
                error(string.Format("Expected '(' instead of '{0}'",token.value));
            nextToken();
            if (!match(new string[] {"NAME",}))
                error(string.Format("Expected sense name instead of '{0}'",token.value));
            name = token.value;
            nextToken();
            if (!match(new string[] {"RPAREN",}))
            {
                value = getValue();
                if (!match(new string[] {"RPAREN",}))
                    pred = getPredicate();
            }
            if (!match(new string[] {"RPAREN",}))
                error(string.Format("Expected ')' instead of '{0}'",token.value));
            nextToken();

            return new Tuple<string,string,string>(name,value,pred);
        }

        /// <summary>
        /// value ::= NUMINT | NUMFLOAT | NAME
        /// </summary>
        /// <returns>The value as string.</returns>
        public string getValue()
        {
            if (!match(new string[] {"NUMINT","NUMFLOAT","NAME","STRINGVALUE","NIL"}))
                error(string.Format("Expected a valid sense value " +
                    " instead of '{0}'", token.value));
            string value = token.value;
            nextToken();

            return value;
        }

        /// <summary>
        /// predicate ::= PREDICATE
        /// </summary>
        /// <returns>The predicate as a string.</returns>
        public string getPredicate()
        {
            if (!match(new string[] {"PREDICATE",}))
                error(string.Format("Expected a valid sense predicate " +
                    " instead of '{0}'", token.value));
            string pred = token.value;
            nextToken();

            return pred;
        }

        /// <summary>
        /// freq ::= <freq-unit> <numfloat> ")"
        /// </summary>
        /// <returns>frequency as period time</returns>
        public long getFreq()
        {
            string unit = getFreqUnit();
            float value = getNumFloat();
            if (!match(new string[] {"RPAREN",}))
                error(string.Format("Expected ')' instead of '{0}'",token.value));
            nextToken();
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
        public string getFreqUnit()
        {
            if (!match(new string[] {"HOURS", "MINUTES", "SECONDS", "HZ", "PM", "NONE"}))
                error(string.Format("Expected a valid frequency unit " +
                    "instead of '{0}'",token.value));
            string unit = token.token;
            nextToken();

            return unit;
        }

        /// <summary>
        /// time ::= <time-unit> <numfloat> ")"
        /// </summary>
        /// <returns>time in milliseconds</returns>
        public long getTime()
        {
            string unit = getTimeUnit();
            float value = getNumFloat();

            if (!match(new string[] {"RPAREN",}))
                error(string.Format("Expected ')' instead of '{0}'",token.value));
            nextToken();
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
        /// time-unit ::= HOURS | MINUTES | SECONDS | NONE
        /// </summary>
        /// <returns>The unit as token string.</returns>
        public string getTimeUnit()
        {
            if (!match(new string[] {"HOURS", "MINUTES","SECONDS","NONE"}))
                error(string.Format("Expected a valid time unit " +
                    "instead of '{0}'",token.value));
            string unit = token.value;
            nextToken();

            return unit;
        }

        /// <summary>
        /// numfloat ::= NUMINT | NUMFLOAT
        /// </summary>
        /// <returns>The number as float.</returns>
        public float getNumFloat()
        {
            if (!match(new string[] {"NUMINT", "NUMFLOAT"}))
                error(string.Format("Expected a floating-point number " +
                    "instead of '{0}'",token.value));
            string value = token.value;
            nextToken();

            return float.Parse(value);
        }

        /// <summary>
        /// opt-comment ::= COMMENT |
        /// </summary>
        public void getOptComment()
        {
            if (match(new string[] {"COMMENT",}))
                nextToken();
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
