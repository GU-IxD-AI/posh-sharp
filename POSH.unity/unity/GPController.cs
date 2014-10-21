using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using POSH.sys;
using System.IO;
using GrammarGP.env;




namespace POSH.unity
{
    public abstract class GPController : POSHController
    {


        //TODO: currently disabled because it would need further input to make a good UI for adding additional props 
        //public bool use_Agent_configuration;
        //public POSH.sys.IBehaviourConnector.AgentParameter[] agentConfigurations;
        GPSystem gpSystem;

        protected void InitPOSH()
        {
#if LOG_ON
            string configFile = Application.dataPath + String.Format("{0}POSH{0}lib{0}log4net.xml",Path.DirectorySeparatorChar);
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                configFile = Application.dataPath + "\\log4net.xml";
            }

            /**/
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(configFile);
            /**/
            log4net.Config.XmlConfigurator.ConfigureAndWatch(fileInfo);
            /**/
            log4net.LogManager.GetLogger(typeof(LogBase)).InfoFormat("tesat", configFile);
#endif 

            AssemblyControl.SetForUnityMode();
            poshLink = AssemblyControl.GetControl() as EmbeddedControl;
            poshLink.SetBehaviourConnector(this);

            plans = CreatePOSHDict(actionPlans);
            poshLink.SetActionPlans(plans);

            initFiles = CreatePOSHDict(agentConfiguration);
            poshLink.SetInitFiles(initFiles);
          
            engineLog = "init";
            

        }

        /// <summary>
        /// INIT for the GP system. 
        /// Uses the originally provided plans to come up with a first gene and chromosome pool
        /// </summary>
        /// <param name="agentInit"></param>
        /// <returns></returns>
        private bool InitGP(List<Tuple<string, object>> agentInit)
        {
            gpSystem = new GPSystem();
            // TODO: the seed needs to be made reasonable
            gpSystem.SetRandomSeed(DateTime.Now.Millisecond*DateTime.Now.Minute*DateTime.Now.Hour);
            
            List<Tuple<string,string>> basePlans = new List<Tuple<string,string>>();

            foreach (Tuple<string, object> agents in agentInit)
            {
                Tuple<string, string> pair = new Tuple<string, string>(name, plans[name]);
                if (!basePlans.Contains(pair))
                    basePlans.Add(pair);
            }
            
            return gpSystem.InitGPWithPlan(basePlans.ToArray());
        }

        private void StartGPSystem(AgentBase[] agents)
        {
            // TODO: here is the perfect place to bring in a listener to use for evolution and simulation of parallel strains of the same agent
            // we can use poshLink.RelinkAgent(agent,newPlan) for that and beforehand add a new plan to poshLink.AddPlan()
            // the listener will be in the programchromosome which contains the agent and the plan
            gpSystem.Configure(50, agents.Length, 150, 0.01f, 0.2f, true);
            
            gpSystem.ConnectAgents(agents);
            // TODO: the gpSystem needs to be told that it has all neded info now 
            gpSystem.Start();
   
        }

        /// <summary>
        /// Checks if at least one POSH agent is still running
        /// </summary>
        /// <param name="checkStopped">If true the method checks if the agents are entirely stopped if false it will check if the agents are only paused.</param>
        /// <returns></returns>
        protected bool AgentRunning(bool checkStopped)
        {
            foreach (AgentBase agent in agents)
                if (checkStopped)
                {
                    if (agent.LoopStatus().First)
                        return true;
                }
                else
                {
                    if (agent.LoopStatus().Second)
                        return true;
                }

            return false;
        }

        protected bool RunPOSH()
        {
            if (started)
                return true;

            List<Tuple<string, object>> agentInit = poshLink.InitAgents(true, "", usedPOSHConfig);
            Debug.Log("init GP");
            InitGP(agentInit);

            Debug.Log("init POSH");
            agents = poshLink.CreateAgents(true, usedPOSHConfig, agentInit, new Tuple<World, bool>(null, false));

            StartGPSystem(agents);

            poshLink.StartAgents(true, agents);
            poshLink.Running(true, agents, false);
            Debug.Log("running POSH");
            started = true;
            return started;
        }

        /// <summary>
        /// The idea is to this call for switching plans for agents and update them.
        /// It would also be good to use this signal as an evaluation point of the fitness for each agent.
        /// 
        /// The method itself takes some time to compute as it fires up the gp so using it within a separate thread or co-routine would be advantageous.
        /// </summary>
        /// <returns></returns>
        protected void EvaluateAgents()
        {
            // COMMENT: a clever thing would be to check if the agent is visible to the player and then only update those who are not visible
            // but that is something for the future

            bool result = gpSystem.EvaluateAgents();
            gpSystem.Evolve();

        }

        protected void UpdateAgents(int number)
        {
            Tuple<string,string> [] plans = gpSystem.GetEvolvedPlans(number);

            for (int i = 0;i< plans.Length; i++)
                poshLink.AddActionPlan(plans[i].First,plans[i].Second);


            // TODO: this needs to be re-worked as the newly linked plan mostl likely should have a different name otherwise we would override
            // all plans in the controller
            foreach (AgentBase ag in agents)
                poshLink.ReLinkAgents(ag,ag.linkedPlanName);
        }



        public sys.Behaviour[] GetBehaviours(AgentBase agent)
        {
            List<sys.Behaviour> result = new List<sys.Behaviour>();

            foreach (POSHBehaviour behave in this.behaviourPool)
                result.Add(behave.LinkPOSHBehaviour(agent));

            return result.ToArray();
        }

        public string GetPlanFileStream(string planName)
        {
            return plans[planName];
        }

        public string GetInitFileStream(string libraryName)
        {
            return initFiles[libraryName];
        }

        public bool Ready()
        {
            if (poshLink != null && behaviourPool.Count() > 0 && 
                actionPlans.Length > 0 && agentConfiguration.Count() > 0 && 
                usedPOSHConfig.Length > 1)
                return true;

            return false;
        }

    }
}
