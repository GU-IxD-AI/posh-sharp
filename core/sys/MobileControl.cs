using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Reflection;
using System.IO;
using log4net;

namespace POSH_sharp.sys
{
    public class MobileControl : AssemblyControl
    {
        int agentId = 0;
        protected Dictionary<string,Stream> action_plans;
        protected IBehaviourConnector connector;
        protected Dictionary<string, Stream> initFile;
        protected BehaviourDict behaviours;
                

        internal MobileControl() : base()
        {
        }
        public override BehaviourDict GetBehaviours(string lib,AgentBase agent)
		{
			return GetBehaviours (lib, null,agent);
		}

        protected internal void SetActionPlans(Dictionary<string,Stream> plans) {
            action_plans = plans;
        }

        protected internal void SetBehaviourConnector(IBehaviourConnector connector)
        {
            this.connector = connector;
        }

        protected internal void SetInitFiles(Dictionary<string, Stream> initFiles)
        {
            initFile = initFiles;
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
            Behaviour[] behaviours=null;
            if (log is ILog)
                log.Debug("Scanning library "+lib+" for behaviour classes");

            if (this.connector is IBehaviourConnector)
                behaviours = this.connector.GetBehaviours(agent);
            
            if (behaviours != null)
                foreach (Behaviour behave in behaviours)
                    if (behave != null && behave.GetType().IsSubclassOf(typeof(Behaviour)))
                        dict.RegisterBehaviour(behave);
                
            

            return dict;
        }

        public override StreamReader GetAgentInitFileStream(string agentsInitFile)
        {
            return GetAgentInitFileStream(null,agentsInitFile);
            
        }

        public override StreamReader GetAgentInitFileStream(string assembly, string agentsInitFile)
        {
            Stream initStream;

            if (this.initFile.ContainsKey(agentsInitFile))
                initStream = this.initFile[agentsInitFile];
            else
                initStream = Stream.Null;
            
            return new StreamReader(initStream);
            
        }

        /// <summary>
        /// Returns the plan file name for the given library and plan
        /// </summary>
        /// <param name="lib">The library that the plan is from</param>
        /// <param name="plan">The name of the plan (without the .lap ending)</param>
        /// <returns>The filename with full path of the plan</returns>
        new internal Stream GetPlanFile(string lib, string plan)
        {
            Stream planStream;

            if (this.action_plans.ContainsKey(plan))
                planStream = this.action_plans[plan];
            else
                planStream = Stream.Null;

            return planStream;
        }
    }




}
