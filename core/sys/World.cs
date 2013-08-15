using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys
{
    public class World
    {
        /// <summary>
        /// Returns the behaviour library name that the agents are to use.
        /// </summary>
        public string library { get; private set; }

        /// <summary>
        /// Returns the arguments for customised world initialisation.
        /// 
        /// If no arguments are given, None is returned.
        /// </summary>
        public string args { get; private set; }

        /// <summary>
        /// Returns the agents initialisation structure.
        /// </summary>
        public List<Tuple<string, object>> agentsInit { get; private set; }
        public bool createsAgents { get; private set; }


		public World(string library) : this (library, null, null)
		{}
        /// <summary>
        /// The World class that is used to communicate with the world
        /// initialisation script.
        /// 
        /// Upon running the world initialisation script, using the L{run_world_script},
        /// an instance of this class, named 'world' is given to the script. The
        /// script can use this instance to gather information on how the world is to
        /// be initialised, and can return the world object and other information to
        /// the instance that calls this script.
        /// </summary>
        /// <param name="library">name of the behaviour library that is to be used.</param>
        /// <param name="world_args">arguments to be given to the world initialisation script.</param>
        /// <param name="agentsInit">structure containing information to initialise the agents. as returned by
        /// L{POSH.agentinitparser.parse_agent_init_file}</param>
        public World(string library, string worldArgs, List<Tuple<string, object>> agentsInit)
        {
            this.library = library;
            this.args = worldArgs;
            this.agentsInit = (agentsInit == null) ? new List<Tuple<string, object>>() { } : agentsInit;
            this.createsAgents = false;

        }

        /// <summary>
        /// Specifies that the agents are created an run by the world
        /// initialisation script.
        /// 
        /// By default, the world initialisation script is only responsible for
        /// setting up the world, and eventually returning the world object to
        /// initialise the agents with. Calling this method from the world
        /// initialisation script indicates that both creation and running the
        /// agents is performed by the world initialisation script. For this
        /// purpose, the script can use L{library()} and L{agentsInit()}.
        /// </summary>
        void createAgents()
        {
            this.createsAgents = true;
        }

    }
}
