using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys.exceptions;
using System.IO;
using log4net.Core;
using System.Threading;
using POSH_sharp.sys;

namespace POSH_sharp.executing
{
    /// <summary>
    /// Launches a POSH agent or a set of agents.
    /// 
    /// 
        /// Synopsis:
    ///     launch.py [OPTIONS] library

    /// Description:
    ///     Launches a POSH agent by fist initialising the world and then the
    ///     agents. The specified library is the behaviour library that will be used.
    /// 
    ///     -v, --verbose
    ///         writes more initialisation information to the standard output.
    /// 
     ///    -h, --help
    ///         print this help message.
    /// 
    ///     World initialisation:
    /// 
    ///     -w, --init-world-file=INITSCRIPT
    ///         the python script that initialises the world. To communicate with
    ///         launch.py, an instance of class World called 'world' is passed to the
    ///         world initialisation script. Its most important methods:
    ///             world.args() : Returns the arguments given by the -a options.
    ///                 If -a is not given, None is returned.
    ///             world.set(x) : Passes x as the world object to the agents upon
    ///                 initialising them.
    ///             world.createsAgents() : Needs to be called if the
    ///                 world initialisation script rather than launch.py creates
    ///                 and runs the agents.
    ///         More information on the World class can be found in the API
    ///         documenatation of the POSH.utils.World class.
    ///         If no world initialisation script is specified, then the default world
    ///         initialisation function of the library is called.
    /// 
    ///     -a, --init-world-args=ARGS
    ///         the argument string given to the function init_world(args) in the
    ///         script specified by -w. If no such script is given, the the arguments
    ///         are given to the default world initialisation function of the library.
    /// 
    ///     Agent initialisation:
    ///         If none of the below options are given, then the default library
    ///         initialisation file is used to initialise the agent(s).
    /// 
    ///     -i, --init-agent-file=INITFILE
    ///         initialises the agent(s) according to the given file. The file format
    ///         is described below.
    /// 
    ///     -p, --plan-file=PLANFILE
    ///         initialises a single agent to use the given plan. Only the name of
    ///         the plan without the path needs to be given, as it is assumed to have
    ///         the ending '.lap' and reside in the default location in the
    ///         corresponding behaviour library. This option is only valid if -i 
    ///         is not given.

    /// Agent initialisation file format:
    ///     The agent initialisation file allows the initialisation of one or several
    ///     agents at once. The file is a simple text file that is read line by line.
    ///     Each new agents starts with a '[plan]' line that specifyies the plan that
    ///     the agent uses. This is followed by a list of attributes and values to
    ///     initialise the behaviours of the agent. Empty lines, and lines starting
    ///     with '#' are ignored.
    /// 
    ///     An example file would be:
    ///     
    ///         [plan1]
    ///         beh1.x = 10
    ///         beh2.y = 20
    ///     
    ///         [plan2]
    ///         beh1.x = 20
    /// 
    ///     This file initialises two agents, one with plan1 and the other with plan2.
    ///     Additionally, the attribute 'x' of behaviour 'beh1' of the first agent is
    ///     set to 10, and attribute 'y' of behaviour 'beh2' to 20. For the second
    ///     agent, the attribute 'x' of behaviour 'beh1' is set to 20.
    /// </summary>
    class Launcher
    {
        const string helpText =@"
          Launches a POSH agent or a set of agents.
     
     
             Synopsis:
             launch.py [OPTIONS] library

         Description:
             Launches a POSH agent by fist initialising the world and then the
             agents. The specified library is the behaviour library that will be used.
     
             -v, --verbose
                 writes more initialisation information to the standard output.
     
             -h, --help
                 print this help message.
     
             World initialisation:
     
             -w, --init-world-file=INITSCRIPT
                 the python script that initialises the world. To communicate with
                 launch.py, an instance of class World called 'world' is passed to the
                 world initialisation script. Its most important methods:
                     world.args() : Returns the arguments given by the -a options.
                         If -a is not given, None is returned.
                     world.set(x) : Passes x as the world object to the agents upon
                         initialising them.
                     world.createsAgents() : Needs to be called if the
                         world initialisation script rather than launch.py creates
                         and runs the agents.
                 More information on the World class can be found in the API
                 documenatation of the POSH.utils.World class.
                 If no world initialisation script is specified, then the default world
                 initialisation function of the library is called.
     
             -a, --init-world-args=ARGS
                 the argument string given to the function init_world(args) in the
                 script specified by -w. If no such script is given, the the arguments
                 are given to the default world initialisation function of the library.
     
             Agent initialisation:
                 If none of the below options are given, then the default library
                 initialisation file is used to initialise the agent(s).
     
             -i, --init-agent-file=INITFILE
                 initialises the agent(s) according to the given file. The file format
                 is described below.
     
             -p, --plan-file=PLANFILE
                 initialises a single agent to use the given plan. Only the name of
                 the plan without the path needs to be given, as it is assumed to have
                 the ending '.lap' and reside in the default location in the
                 corresponding behaviour library. This option is only valid if -i 
                 is not given.

         Agent initialisation file format:
             The agent initialisation file allows the initialisation of one or several
             agents at once. The file is a simple text file that is read line by line.
             Each new agents starts with a '[plan]' line that specifyies the plan that
             the agent uses. This is followed by a list of attributes and values to
             initialise the behaviours of the agent. Empty lines, and lines starting
             with '#' are ignored.
     
             An example file would be:
         
                 [plan1]
                 beh1.x = 10
                 beh2.y = 20
         
                 [plan2]
                 beh1.x = 20
     
             This file initialises two agents, one with plan1 and the other with plan2.
             Additionally, the attribute 'x' of behaviour 'beh1' of the first agent is
             set to 10, and attribute 'y' of behaviour 'beh2' to 20. For the second
             agent, the attribute 'x' of behaviour 'beh1' is set to 20.";

        internal WorldControl control;

        /// <summary>
        /// Parses the command line options and returns them.
        /// 
        /// The are returned in the order help, verbose, world_file, world_args,
        /// agent_file, plan_file. help and verbose are boolean variables. All the
        /// other variables are strings. If they are not given, then an empty string
        /// is returned.
        /// </summary>
        /// <param name="argv"></param>
        /// <returns></returns>
        /// <exception cref="UsageException"> whenever something goes wrong with the input string</exception>
        protected Tuple<bool,bool,string,string,string,string,string>  ProcessOptions(string [] args)
        {
            // default values
            bool help = false, verbose = false;
            string worldFile = "", worldArgs = "", agentFile = "", planFile = "", library = "";

            // parse options

            for(int i = 0; i < args.Length - 1; i++)
            {   
                string [] tuple = args[i].Split(new string [] {"="},2,StringSplitOptions.None);
                switch (tuple[0])
                {
                    case "-h":
                    case "--help":
                        help = true;
                        break;
                    case "-v":
                    case "--verbose":
                        verbose = true;
                        break;
                    case "-w":
                    case "--init-world-file":
                        worldFile = tuple[1];
                        break;
                    case "-a":
                    case "--init-world-args":
                        worldArgs = tuple[1];
                        break;
                    case "-i":
                    case "--init-agent-file":
                        agentFile = tuple[1];
                        break;
                    case "-p":
                    case "--plan-file":
                        planFile = tuple[1];
                        break;
                    default:
                        throw new UsageException("unrecognised option: " + tuple[0]);
                }
            }
            if (help)
                return new Tuple<bool,bool,string,string,string,string,string>(help,false,"","","","","");
            // get library from only arguments
            if (args[args.Length].StartsWith("-"))
                throw new UsageException("requires one and only one argument (the library); plus optional options");
                
            library = args[args.Length];
            if (!control.isLibrary(library))
                throw new UsageException(string.Format("cannot find specified library '{0}'",library));
                
            // check for option consistency
            if (agentFile != string.Empty && planFile != string.Empty)
                throw new UsageException("agent initialisation file and plan file cannot be" +
                    "specified simultaneously");

            if (planFile != string.Empty && !control.isPlan(library,planFile))
                throw new UsageException(string.Format("cannot find specified plan '{1}' in library '{0}'",library,planFile));
                
            if (agentFile ==string.Empty && planFile==string.Empty && control.defaultAgentInit(library) == string.Empty )
                throw new UsageException(string.Format("no default agent initialisation file for"+
                    "library '{0}', please specify one",library));
         
            // all fine
            return new Tuple<bool,bool,string,string,string,string,string>(help,verbose,worldFile,worldArgs,agentFile,planFile,library);
        }

        /// <summary>
        /// Calls WorldControl.run_world_script() to initialise the world and returns the
        /// wordls object.
        /// </summary>
        /// <param name="worldFile"></param>
        /// <param name="library"></param>
        /// <param name="worldArgs"></param>
        /// <param name="agentsInit"></param>
        /// <param name="?"></param>
        protected Tuple<World,bool> InitWorld(string worldFile, string library, string worldArgs, Dictionary<string,object> agentsInit, bool verbose)
        {
            if (verbose)
                Console.Out.WriteLine("- initialising world");
            if (worldArgs.Trim() == string.Empty)
                worldArgs = null;

            // find which world script to run
            if (worldFile.Trim() != string.Empty)
            {
                
                if (verbose)
                    Console.Out.WriteLine(string.Format("running '{0}'", worldFile));
                return control.runWorldScript(new Tuple<string,string>(Path.GetDirectoryName(worldFile),
                    Path.GetFileName(worldFile)),library,worldArgs,agentsInit);
            }
            if (verbose)
                Console.Out.WriteLine("no default world initialisation script");

            return new Tuple<World,bool>(null,false);
        }

        private void InitAgent(bool verbose, ref string agentFile, string planFile, string library, ref Dictionary<string, object> agentsInit)
        {
            if (planFile == string.Empty)
            {
                if (agentFile == string.Empty)
                    agentFile = control.defaultAgentInit(library);
                if (verbose)
                    Console.Out.WriteLine(string.Format("reading initialisation file '{0}'", agentFile));
                try
                {
                    agentsInit = AgentInitParser.initAgentFile(library);
                }
                catch (Exception e)
                {
                    try
                    {
                        agentsInit = AgentInitParser.initAgentFile(control.getLibraryFile(library, agentFile));
                    }
                    catch (Exception)
                    {
                        Console.Out.WriteLine("reading agent initialisation file failed");
                        if (verbose)
                            Console.Out.WriteLine(e);
                    }
                }
            }
            else
            {
                if (verbose)
                    Console.Out.WriteLine(string.Format("create single agent with plan '{0}'", planFile));
                agentsInit = new Dictionary<string, object>();
                agentsInit.Add(planFile, null);
            }
        }

        private AgentBase[] createAgents(bool verbose, string library, Dictionary<string, object> agentsInit, Tuple<World, bool> setting)
        {
            // create the agents
            AgentBase[] agents = null;
            if (verbose)
                Console.Out.WriteLine("- creating agent(s)");
            try
            {
                agents = AgentFactory.createAgents(library, "", agentsInit, setting.First);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("creating agent(s) failed, see following error");
                Console.Out.WriteLine("----");
                if (verbose)
                    Console.Out.WriteLine(e);
            }
            return agents;
        }

        private bool Run(bool verbose, AgentBase[] agents, bool loopsRunning)
        {
            // check all 0.1 seconds if the loops are still running, and exit otherwise
            while (loopsRunning)
            {
                Thread.Sleep(100);
                loopsRunning = false;
                foreach (AgentBase agent in agents)
                    if (agent.loopStatus().First)
                        loopsRunning = true;
            }
            if (verbose)
                Console.Out.WriteLine("- all agents stopped");
            return loopsRunning;
        }

        private bool StartAgents(bool verbose, AgentBase[] agents)
        {
            if (verbose)
                Console.Out.WriteLine("- starting the agent(s)");
            if (agents is AgentBase[])
                foreach (AgentBase agent in agents)
                    agent.startLoop();

            return true;
        }

        public static void Main(string [] args)
        {
            // OLDCOMMENT: There must be a beter way to do this... see jyposh.py and utils compile_mason_java() 

            bool help = false, verbose = false;
            string worldFile = "", worldArgs = "", agentFile = "", planFile = "", library = "";
            Dictionary<string, object> agentsInit = null;
            Tuple<World, bool> setting = null;
            AgentBase[] agents = null;

            // process command line arguments
            Launcher application = new Launcher();

            Tuple<bool,bool,string,string,string,string,string> arguments = null;
            if (args is string[] && args.Length > 0)
                arguments = application.ProcessOptions(args);
            else
            {
                Console.Out.WriteLine("for help use --help");
                return;
            }
            if (arguments != null && arguments.First)
            {
                Console.Out.WriteLine(helpText);
                return;
            }

            help = arguments.First;
            verbose = arguments.Second;
            worldFile = arguments.Third;
            worldArgs = arguments.Forth;
            agentFile = arguments.Fifth;
            planFile = arguments.Sixth;
            library = arguments.Seventh;
            
            // activate logging. we do this before initialising the world, as it might
            // use this logging facility

            if (verbose)
                StreamLogger.setupConsoleLogging(Level.Debug);
            else
                StreamLogger.setupConsoleLogging(Level.Info);
            // read agent initialisation. this needs to be done before initialising
            // the world, as the agent initialisation needs to be give to the world
            // initialisation script

            if (verbose)
                Console.Out.WriteLine("- collect agent initialisation options");
            application.InitAgent(verbose, ref agentFile, planFile, library, ref agentsInit);

            if (verbose)
                Console.Out.WriteLine(string.Format("will create {0} agent(s)", agentsInit.Count));

            // init the world
            if (worldFile == string.Empty)
                worldFile = application.control.defaultWorldScript(library);
            try
            {
                setting = application.InitWorld(worldFile, library, worldArgs, agentsInit, verbose);
            }
            catch (Exception e)
            {
                try
                {
                    setting = application.InitWorld(application.control.getLibraryFile(library,worldFile), 
                        library, worldArgs, agentsInit, verbose);
                }
                catch (Exception)
                {
                    Console.Out.WriteLine("world initialisation failed");
                    Console.Out.WriteLine("-------");
                    if (verbose)
                        Console.Out.WriteLine(e);
                    
                }
            }

            if (setting != null && setting.Second)
            {
                if (verbose)
                    Console.Out.WriteLine("- world initialisation script indicated that it created " +
                        "agents. nothing more to do.");
                return;
            }

            agents = application.createAgents(verbose, library, agentsInit, setting);
            if (agents == null)
                return;
            // start the agents
            bool loopsRunning = application.StartAgents(verbose, agents);

            loopsRunning = application.Run(verbose, agents, loopsRunning);

        }



        


    }
}
