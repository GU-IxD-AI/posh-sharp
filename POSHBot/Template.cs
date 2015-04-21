using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using POSH.sys.annotations;
using Posh_sharp.POSHBot.util;
using POSH.sys.strict;
//import utilityfns

namespace Posh_sharp.POSHBot
{
    /// <summary>
    /// This class presents a template for creating a new Behaviour for UT2004 based agents. It is not intended to be used as is but modified and renamed.
    /// Once your own UTBehaviour is created by renaming this class you will receive ann information about the virtual environment at runtime and can execute actions. 
    /// </summary>
    public class Template : UTBehaviour
    {
        internal CombatInfo info;

        public Template(AgentBase agent)
            :base(agent,new string[] {"TemplateAction1ToBlock",
                            "TemplateAction2ToBlock"},
                        new string[] {"TemplateSense1ToBlock",
                            "TemplateSense2ToBlock"})
        {
            info = new CombatInfo();
        }
            
        /*
         * 
         * OTHER FUNCTIONS 
         * 
         */

        private void TemplateInnerMethod()
        {
            
        }

 

        override internal void ReceiveProjectileDetails(Dictionary<string,string> values)
        {
        }

        override internal void ReceiveDamageDetails(Dictionary<string,string> values)
        {
        }

        /// <summary>
        /// handle details about a player (not itself) dying
        /// remove any info about that player from CombatInfo
        /// </summary>
        /// <param name="values"></param>
        override internal void ReceiveKillDetails(Dictionary<string,string> values)
        {
        }

        override internal void ReceiveDeathDetails(Dictionary<string,string> value)
        {
        }

        /*
        * 
        * ACTIONS 
        * 
        */

        /// <summary>
        /// This is a template action that can be used to modify the agent behaviour.
        /// Actions always return true or false and should be the actuators of an agent.
        /// </summary>
        /// <returns>True or false, dependent if the action was executed successful.</returns>
        [ExecutableAction("TemplateAction1")]
        public bool TemplateAction1()
        {
            if (_debug_)
                Console.Out.WriteLine(" in TemplateAction1");

            // This is an example command which sends a request to the game engine to let the character stop shooting.
            // The commands are based on the included GameBots2004 API which is available on the project webpage.
            // GetBot().SendMessage("STOPSHOOT", new Dictionary<string, string>());
            return false;
        }

        [ExecutableAction("TemplateAction2")]
        public bool TemplateAction2()
        {
            if (_debug_)
                Console.Out.WriteLine(" in TemplateAction1");
            return false;
        }
        [ExecutableAction("TemplateAction3",0.1f)]
        public bool TemplateAction3()
        {
            if (_debug_)
                Console.Out.WriteLine(" in TemplateAction1");
            return false;
        }
        [ExecutableAction("TemplateAction3",0.2f)]
        public bool TemplateAction3b()
        {
            if (_debug_)
                Console.Out.WriteLine(" in TemplateAction1");
            return false;
        }

        /*
         * 
         * SENSES
         * 
         */

        /// <summary>
        /// This is a template sense that can be used to modify the agent behaviour.
        /// Senses are used to retrieve information about the environment the agent is in. They do not modify the environment but rather observe.
        /// Senses should be rather lightweight as they are called frequently, meaning they are not good for heavy computation except for special 
        /// cases where a sense is called rather seldom.
        /// </summary>
        /// <returns>Senses can return bools, ints, floats and longs but should not return complex objects. If you are in need of complex 
        /// information about the world you should store those inside the behaviour and let actions read them directly.</returns>
        [ExecutableSense("TemplateSense1")]
        public bool TemplateSense1()
        {
            if (_debug_)
                Console.Out.WriteLine(" in TemplateSense1");
            return false;
        }

        [ExecutableSense("TemplateSense2")]
        public int TemplateSense2()
        {
            if (_debug_)
                Console.Out.WriteLine(" in TemplateSense2");
            return 0;
        }
    }
}     

    
