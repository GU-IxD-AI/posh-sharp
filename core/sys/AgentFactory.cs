﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.strict;
using System.IO;

namespace POSH.sys
{
    /// <summary>
    /// Functions to create new agents.
    /// 
    /// The functions were taken out of the utils module to avoid cyclic imports. @swen: (One might place them again into WorldControl)
    /// </summary>
    public class AgentFactory
    {
        //from utils import get_plan_file
        //# agent bases
        //from strict import Agent as StrictAgent
        //from scheduled import Agent as ScheduledAgent

        // private constants
        public enum PLANTYPE {DC, SDC, RDC, SRDC,NONE};

        private struct AGENTTYPE
        {
            public static Type getType(PLANTYPE type)
            {
                switch (type)
                {
                    case PLANTYPE.DC:
                        return typeof(scheduled.Agent);
                    case PLANTYPE.RDC:
                        return typeof(scheduled.Agent);
                    case PLANTYPE.SDC:
                        return typeof(strict.Agent);
                    case PLANTYPE.SRDC:
                        return typeof(strict.Agent);
                    default:
                        return null;
                }
            }
        }
        //_agent_types = {'DC' : ScheduledAgent,
        //                'RDC' : ScheduledAgent,
        //                'SDC' : StrictAgent, 
        //                'SRDC' : StrictAgent }


        /// <summary>
        /// Returns the type of the plan of the given plan file.

        /// The type is returned as a string (e.g. 'SDC', 'RDC', SRDC'). If the type was not
        /// found then an empty string is returned. The plan has to be given without
        /// its file ending and has to be in the PLANPATH directory in the
        /// corresponding library.
        ///
        /// The function parses the plan line by line until it finds a plan type
        /// identifier after a '('. This function can fail in several ways:
        ///    - There is a comment that has '(DC' or similar
        ///    - The bracket is on the line before the identifier
        ///    - Other ways that I haven't though about
        /// </summary>
        /// <param name="planFile"> Filename of the plan file</param>
        /// <returns>Type of plan, or '' if not recognised</returns>
        public static PLANTYPE getPlanType(string planFile)
        {
            try
            {
                string[] reader = planFile.Split(Environment.NewLine.ToCharArray());
                
                foreach(string line in reader)
                {
                    foreach (PLANTYPE planID in Enum.GetValues(typeof(PLANTYPE)))
                    { 
                        int idPos = line.IndexOf(planID.ToString(""));
                        if (idPos == -1)
                            continue;
                        // is there a bracket before?
                        int bracketPos = line.IndexOf("(");
                        if (bracketPos == -1 || bracketPos > idPos )
                            continue;
                        //  only valid if there is nothing else than whitespaces between
                        //  the plan identifier and the bracket
                        if (idPos - bracketPos == 1 || line.Substring(bracketPos,idPos-bracketPos).Trim() == string.Empty)
                            return planID;
                    }
                }
  
            }
            catch (IOException)
            {
                return PLANTYPE.NONE;
            }
            return PLANTYPE.NONE;
        }

		public static AgentBase[] CreateAgents(string assembly)
		{
			return CreateAgents (assembly,"",null,null);
		}
        /// <summary>
        /// Returns a sequence of newly created agents using the given behaviour
        /// library.
        /// 
        /// The type of agents are determined by their plan type, using the
        /// getPlanType() function. If a world object is given, it is given to the
        /// agent upon initialisation.
        ///
        /// The function must be given either a plan file of an agents_init structure,
        /// but not both. If a plan is given, then a single agent is created and
        /// returned as a sequence of one element. agents_init is a structure as
        /// returned by the agentinitparser module and allows creating and
        /// initialisation of several agents. The agents are created one by one
        /// and returned as a sequence. If both a plan and agents_init are given,
        /// then the plan is ignored.
        /// </summary>
        /// <param name="assemblyName">name of the library</param>
        /// <param name="plan">name of the plan (without path and file ending)</param>
        /// <param name="agentsInit">data structure for agent initialisation
        /// as returned by AgentInitParser.initAgentFile. The first element is the plan file name the second element is an attribute dictionary</param>
        /// <param name="world">world object, given to agents at construction</param>
        /// <returns>List of Agents</returns>
        public static AgentBase[] CreateAgents(string assemblyName, string plan, List<Tuple<string, object>> agentsInit, World world)
        {
            // build initialisation structure
            if (agentsInit == null)
            {
                if (plan == string.Empty)
                    throw new TypeLoadException("create_agent() requires either plan or agents_init to be specified");
                agentsInit = new List<Tuple<string, object>>();
                
                /// string for the plan and a dictionary for the
                /// (behaviour, attribute) -> value assignment.
                agentsInit.Add(new Tuple<string,object>(plan, new Dictionary<Tuple<string, string>, object>()));
            }
            // create the agents
            List<AgentBase> agents = new List<AgentBase>();
            foreach (Tuple<string, object> pair in agentsInit)
            {
                string agentPlan = pair.First;
                Dictionary<Tuple<string, string>, object> agentAttributes = (Dictionary<Tuple<string, string>, object>) pair.Second;
                // determine agent type from plan
                PLANTYPE planType = getPlanType(AssemblyControl.GetControl().GetPlanFile(assemblyName, agentPlan));
                if (planType == PLANTYPE.NONE)
                    throw new KeyNotFoundException(string.Format("plan type of plan {0} not recognised", agentPlan));
                Type agentType = AGENTTYPE.getType(planType);
                // create agent and append to sequence

                Type[] constructorTypes = new Type[4];
                constructorTypes[0] = assemblyName.GetType();
                constructorTypes[1] = agentPlan.GetType();
                constructorTypes[2] = agentAttributes.GetType();
                constructorTypes[3] = (world != null) ? world.GetType() : typeof(World);

                System.Reflection.ConstructorInfo constructor = agentType.GetConstructor(constructorTypes);
                agents.Add((AgentBase)constructor.Invoke(new object[] {assemblyName, agentPlan, agentAttributes, world}));
            }
            return agents.ToArray();
        }
    }
}
