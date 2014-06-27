using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using POSH.sys.exceptions;

namespace POSH.sys
{
    /// <summary>
    /// Module to parse agent initialisation file.

    /// The initialisation file has the following format::

    ///     [plan1]
    ///     beh1.attr1 = value1
    ///     beh2.attr1 = value2

    ///     [plan2]
    ///     beh1.attr1 = value3
    ///     beh2.attr2 = value4

    /// Such a file specifies two agents to be creates, one using 'plan1', and the
    /// second using 'plan2'. The first agent is initialised as follows: attribute
    /// attr1 of behaviour beh1 is set to value1, and attribute attr1 of behaviour
    /// beh2 is set to value2. The second agent is initialised in a similar way.

    /// In more detail, the agent initialisation is specified in a line-wise fashion.
    /// Comments (starting with a '#') and empty lines are ignored. Every agent block
    /// in the file start with its plan, given by '[plan]', where 'plan' can be any
    /// string _without_ spaces. After that an aribtrary number of attributes (one per
    /// line) can be given for each behaviour of the agent. An attribute is specified
    /// by 'behaviour.attrribute = value', where 'behaviour' and 'attribute' are
    /// names of the behaviour and the attribute, respectively, and 'value' is the
    /// value that the attribute is set to. The following regular expressions
    /// determine valid names and values::

    ///     behaviour and attribute names  [a-zA-Z_][a-zA-Z0-9_]*
    ///     values:
    ///         integer                    \-?[0-9]+
    ///         float                      \-?(\d*\.\d+|\d+\.)([eE][\+\-]?\d+)?
    ///         boolean                    ([Tt]rue|[Ff]alse)

    /// The values are "automatically" converted into the recognised type. If no type is
    /// recognised, then they are assigned to the attributes as strings.
    /// </summary>
    public class AgentInitParser
    {

        //# symbols
        //_int_matcher = re.compile(r"\-?[0-9]+$")
        //_float_matcher = re.compile(r"\-?(\d*\.\d+|\d+\.)([eE][\+\-]?\d+)?$")
        //_bool_matcher = re.compile(r"([Tt]rue|[Ff]alse)$")
        //_identifier_matcher = re.compile(r"[a-zA-Z_][a-zA-Z0-9_]*$")

        //# structures (whole lines, except for comments)
        //_plan_matcher = re.compile(r"\[(\S+)]$")
        //_attribute_matcher = re.compile(r"(\S+)\.(\S+)\s*=\s*(.+)$")




        // symbols
        public static Regex INTMATCHER =  new Regex("^-?[0-9]*$");
        public static Regex FLOATMATCHER = new Regex(@"^[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)?$");
        public static Regex BOOLMATCHER = new Regex(@"^([Tt]rue|[Ff]alse)$");
        public static Regex IDENTIFIERMATCHER = new Regex(@"[a-zA-Z_][a-zA-Z0-9_]*$");

        // structures (whole lines, except for comments)
        public static Regex PLANMATCHER = new Regex(@"\[(\S+)]$");
        public static Regex ATTRIBUTEMATCHER = new Regex(@"(\S+)\.(\S+)\s*=\s*(.+)$");

        /// <summary>
        /// Converts the given string to the most likely type that it represents.

        /// It tests the types in the following order: int, float, bool. If none of 
        /// these match, then the string is returned as a string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static object strToValue(string s)
        {
            s = s.Trim();
            if (INTMATCHER.IsMatch(s))
                return int.Parse(s);
            if (FLOATMATCHER.IsMatch(s))
                return float.Parse(s);
            if (BOOLMATCHER.IsMatch(s))
                return bool.Parse(s);

            return s;

        }

        /// <summary>
        /// Returns a data structure containing the content of the file.

        /// See the module docstring for the accepted file format. The returned data
        /// structure is a sequence with one entry per agent, with is a pair of a
        /// string for the plan and a dictionary for the
        /// (behaviour, attribute) -> value assignment.
        /// </summary>
        /// <param name="initFile">File path for the agent initialisation.</param>
        /// <returns>Data structure representing content of the file
        /// dictionary (behaviour, attribute) -> value</returns>
        public static List<Tuple<string, object>> initAgentFile(string initFileString)
        {


            string plan = "";
            string[] initFile = initFileString.Split(Environment.NewLine.ToCharArray());
            List<Tuple<string, object>> agentsInit = new List<Tuple<string, object>>();
            Dictionary<Tuple<string, string>, object> currentAttributes = new Dictionary<Tuple<string, string>, object>();
            int lineNr = 0;
            
            foreach(string row in initFile)
            {
                string line = row;
                lineNr++;
                // clean comments, newlines, leading and trailing spaces
                int commentPos = line.IndexOf('#');
                if (commentPos != -1)
                    line = line.Split('#')[0];
                line.Trim();
                if (line != string.Empty)
                {
                    // new agent starts with plan
                    if (PLANMATCHER.IsMatch(line))
                    {
                        if (plan != string.Empty)
                            agentsInit.Add(new Tuple<string,object>(plan,currentAttributes));
                        // removing the brackets around the plan name
                        plan = line.Substring(1, line.Length - 2);
                        currentAttributes = new Dictionary<Tuple<string,string>, object>();
                    }
                    else
                    {
                        // not a plan, needs to be attribute assignment
                        Match matchedAttr = ATTRIBUTEMATCHER.Match(line);
                        if (!(matchedAttr is Match) )
                            throw new AgentInitParseException(string.Format("line {0}: unrecognised syntax '{1}'", lineNr, line));
                        else if (plan == string.Empty)
                            // attributes before plan
                            throw new AgentInitParseException(string.Format("line {0}: [plan] expected", lineNr));
                        string behaviour,attribute,value;
                        behaviour = matchedAttr.Groups[1].Value;
                        attribute = matchedAttr.Groups[2].Value;
                        value = matchedAttr.Groups[3].Value;
                        // check if behaviour and attribute are identifiers
                        if (IDENTIFIERMATCHER.Match(behaviour) == null ||
                            IDENTIFIERMATCHER.Match(attribute) == null)
                            throw new AgentInitParseException(string.Format("line {0}: '{1}.{2}' has incorrect syntax",
                                lineNr, behaviour, attribute));
                        // check what the value could be
                        currentAttributes[new Tuple<string,string>(behaviour,attribute)] =  strToValue(value);
                    }
                }
            }
            if (plan != string.Empty)
                agentsInit.Add(new Tuple<string, object>(plan, currentAttributes));
            if (agentsInit.Count == 0)
                throw new AgentInitParseException("no agents specified in initialisation file");

            return agentsInit;
        }
    }
}
