using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Reflection;
using System.IO;
using POSH.sys.exceptions;
using log4net;

namespace POSH.sys
{
    public class EmbeddedControl : AssemblyControl
    {
        protected IBehaviourConnector connector;

        protected BehaviourDict behaviours;

        /// <summary>
        /// Contains the different lap files used for possible agents.
        /// They key is the planID.
        /// </summary>
        protected Dictionary<string, string> actionPlans;
        

        /// <summary>
        /// The dictionary contains the agents and their internal parameters which will be used. Key of the outer dict is the planID. 
        /// The second dict contains parameters and their values in string representation.
        /// </summary>
        protected Dictionary<string, Dictionary<string, string>> initParameters;
        protected Dictionary<string, string> initFile;
                

        internal EmbeddedControl() : base()
        {
            actionPlans = new Dictionary<string, string>();
            initParameters = new Dictionary<string, Dictionary<string, string>>();

        }



        public override BehaviourDict GetBehaviours(string lib,AgentBase agent)
		{
			return GetBehaviours (lib, null,agent);
		}

        /// <summary>
        /// Returns a sequence of classes, containing all behaviour classes that
        /// are available in a particular library.
        /// 
        /// The method searches the behaviour subclasses by attempting to import
        /// all file in the library ending in .dll, except for the WORLDSCRIPT, and
        /// search through all classes they contain to see if they are derived from
        /// any behaviour class.
        /// 
        /// If a log object is given, then logging output at the debug level is
        /// produced.
        /// </summary>
        /// <param name="lib">Name of the library to find the classes for</param>
        /// <param name="log">A log object</param>
        /// <returns>The dictionary containing the Assembly dll name and the included Behaviour classes</returns>
        public override BehaviourDict GetBehaviours(string lib, ILog log, AgentBase agent)
        {
            BehaviourDict dict = new BehaviourDict();
            Behaviour[] behaviours = null;
            if (log is ILog)
                log.Debug("Scanning library " + lib + " for behaviour classes");

            if (this.connector is IBehaviourConnector)
                behaviours = this.connector.GetBehaviours(agent);

            if (behaviours != null)
                foreach (Behaviour behave in behaviours)
                    if (behave != null && behave.GetType().IsSubclassOf(typeof(Behaviour)))
                        dict.RegisterBehaviour(behave);



            return dict;
        }
        public void SetActionPlans(Dictionary<string,string> plans) {
            actionPlans = plans;
        }

        public void SetBehaviourConnector(IBehaviourConnector connector)
        {
            this.connector = connector;
        }

        public void SetInitFiles(Dictionary<string, string> initFiles)
        {
            initFile = initFiles;
        }



        /// <summary>
        /// Returns the plan file name for the given library and plan
        /// </summary>
        /// <param name="lib">The library that the plan is from</param>
        /// <param name="plan">The name of the plan (without the .lap ending)</param>
        /// <returns>The filename with full path of the plan</returns>
        override internal string GetPlanFile(string lib, string plan)
        {
            string planStream;

            if (this.actionPlans.ContainsKey(plan))
                planStream = this.actionPlans[plan];
            else
                planStream = string.Empty;

            return planStream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="verbose"></param>
        /// <param name="assembly"></param>
        /// <param name="agentLibrary"></param>
        /// <returns>returns a dictionary containing agentnames and a dictionary containing attributes for the agent</returns>
        public override List<Tuple<string, object>> InitAgents(bool verbose, string assembly, string agentLibrary)
        {

            if (connector == null || !connector.Ready())
                return null;

            List<Tuple<string, object>> agentsInit = null;
            string agentsInitFile = string.Format("{0}_{1}", agentLibrary, "init.txt");

            // check if the agent init file exists
            //if (connector.GetInitFileStream(agentsInitFile) == null)
            //    throw new UsageException(string.Format("cannot find specified agent init file in for library '{0}' in the resources",
            //            agentLibrary));

            if (verbose)
                Console.Out.WriteLine(string.Format("reading initialisation file '{0}'", agentsInitFile));
            try
            {
                agentsInit = AgentInitParser.initAgentFile(connector.GetInitFileStream(agentsInitFile));
            }
            catch (Exception e1)
            {
                try
                {
                    agentsInit = AgentInitParser.initAgentFile(connector.GetInitFileStream(agentLibrary));
                }
                catch (Exception e2)
                {
                    //TODO: meaningfull error message regarding the agentinit file which seems to be either corrupt or not linked
                }
            }

            return agentsInit;
        }

    }




}
