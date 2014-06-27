using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using POSH.sys;
using System.IO;




namespace POSH.unity
{
    public abstract class POSHController : MonoBehaviour, IBehaviourConnector
    {
        public string engineLog;

        public POSHBehaviour[] behaviourPool;
        public TextAsset[] actionPlans;

        public string usedPOSHConfig;

        public TextAsset[] agentConfiguration;
        private AgentBase[] agents;

        public bool stopAgents = false;

        //TODO: currently disabled because it would need further input to make a good UI for adding additional props 
        //public bool use_Agent_configuration;
        //public POSH.sys.IBehaviourConnector.AgentParameter[] agentConfigurations;

        

        protected EmbeddedControl poshLink;

        private Dictionary<string,string> plans;
        private Dictionary<string, string> initFiles;

        protected bool started = false;


        protected void InitPOSH()
        {
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


            AssemblyControl.SetForUnityMode();
            poshLink = AssemblyControl.GetControl() as EmbeddedControl;
            poshLink.SetBehaviourConnector(this);

            plans = CreatePOSHDict(actionPlans);
            poshLink.SetActionPlans(plans);

            initFiles = CreatePOSHDict(agentConfiguration);
            poshLink.SetInitFiles(initFiles);
          
            engineLog = "init";
            

        }

        protected bool StopPOSH()
        {
            foreach (AgentBase ag in agents)
                ag.StopLoop();

            started = false;
            return false;
        }

        protected bool PausePOSH()
        {
            foreach (AgentBase ag in agents)
                ag.PauseLoop();
            return false;
        }


        void OnApplicationPause(bool pauseStatus)
        {
            if (started)
                PausePOSH();
        }

        void OnApplicationQuit()
        {
            if (started)
                StopPOSH();
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
            Debug.Log("init POSH");
            agents = poshLink.CreateAgents(true, usedPOSHConfig, agentInit, new Tuple<World, bool>(null, false));
            poshLink.StartAgents(true, agents);
            poshLink.Run(true, agents, false);
            Debug.Log("running POSH");
            started = true;
            return started;
        }

        private Dictionary<string, string> CreatePOSHDict(TextAsset [] files)
        {
            Dictionary<string, string> fileDict = new Dictionary<string, string>();

            foreach (TextAsset file in files)
            {
                fileDict.Add(file.name,file.text);
            }

            return fileDict;
        }

        
        public string GetBehaviourLibrary()
        {
            return usedPOSHConfig;
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
