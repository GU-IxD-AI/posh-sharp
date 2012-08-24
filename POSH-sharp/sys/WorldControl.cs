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
    class World
    {
        /// <summary>
        /// Returns the behaviour library name that the agents are to use.
        /// </summary>
        public string library{get; private set;}

        /// <summary>
        /// Returns the arguments for customised world initialisation.
        /// 
        /// If no arguments are given, None is returned.
        /// </summary>
        public string [] args{get; private set;}

        /// <summary>
        /// Returns the agents initialisation structure.
        /// </summary>
        public Dictionary<string,object> agentsInit{get; private set;}
        public bool createsAgents{get; private set;}

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
        public World(string library, string[] worldArgs=null, Dictionary<string,object> agentsInit = null)
        {
            this.library=library;
            this.args= (worldArgs == null) ? new string[] {} : worldArgs;
            this.agentsInit = (agentsInit == null) ? new Dictionary<string,object>() {} : agentsInit;
            this.createsAgents=false;
            
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



    class WorldControl
    {
        int agentId = 0;
        
        static WorldControl instance;

        public static WorldControl GetControl()
        {
            if (instance is WorldControl)
                return instance;
            else
            {
                instance = new WorldControl();
                return instance;
            }

        }
        



        /// <summary>
        /// Sets the world object for use when initialising the agents.
        /// 
        /// The world object given to this method is given to the agents upon
        /// initialisation.
        /// </summary>
        public World world{private get; set;}

        // HACK: worldScript and AgentInit are from config which I have not found yet. Included both into world, maybe need moving
        
        /// <summary>
        /// worldScript is contained in the dll of a certain AI it is an executable object which sets certain elements
        /// The first element is the contained dll the second the name of the type
        /// </summary>
        public Tuple<string,string> worldScript{get; private set;}
        public string agentInit{get; private set;}
        
        // UNDONE: possible idea is to create class inside the bot.dll containing the config object which holds all variables 
        /// <summary>
        /// config is a file containing a list of different environment variables
        /// </summary>
        public Dictionary<string,object> config{get; set;}




        /// <summary>
        /// Returns the root path that POSH is installed in. Assumes that this
        /// module resides in the root path.
        /// </summary>
        /// <returns>Root path</returns>
        public string getRootPath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        /// <summary>
        /// Returns the path to the behaviour library. If no library is given,
        /// the the base path of the library is returned.
        /// </summary>
        /// <param name="lib">The library to return the path for</param>
        /// <returns>The base library path or the path to the given library</returns>
        string getLibraryPath(string lib="")
        {
            if (lib != "")
                return getRootPath()+Path.PathSeparator+config["LibraryPath"]+Path.PathSeparator+lib;
            else
                return getRootPath()+Path.PathSeparator+config["LibraryPath"];
        }

        /// <summary>
        /// Returns the path to the plans of the given behaviour library.
        /// </summary>
        /// <param name="lib">The library to return the path for</param>
        /// <returns>The path to the plans</returns>
        string getPlanPath(string lib)
        {
            return getLibraryPath(lib)+Path.PathSeparator+config["PlanPath"];
            
        }

        /// <summary>
        /// Returns if the given class object is a subclass of a behaviour class.
        /// </summary>
        /// <param name="o">An object</param>
        /// <returns>If the given class is a subclass of L{POSH.Behaviour}</returns>
        bool isBehaviour(object o)
        {
            return (o.GetType().IsSubclassOf(typeof(Behaviour)));
        }

        /// <summary>
        /// Returns a list of available plans for the given behaviour library.
        /// 
        /// The plans need to be located at library/PLANS and end in PLANENDING.
        /// </summary>
        /// <param name="lib">The library to return the list of plans for</param>
        /// <returns>A list of plans without file ending</returns>
        string [] getPlans(string lib)
        {
            string planPath=getPlanPath(lib);
            string [] plans={};
            List<string> result=new List<string>();
            
            try{
                if (File.Exists(planPath))
                    plans=Directory.GetFiles(planPath,"*.lap",SearchOption.TopDirectoryOnly);
                int end; 
                foreach (string plan in plans)
                {
                    end = plan.ToLower().Contains(".lap") ? plan.ToLower().LastIndexOf(".lap") : 0;
                    result.Add(plan.Remove(end));
                }
            } 
            catch (IOException)
            {
                // TODO: @swen: some clever log or comment here!!!
            }
            return result.ToArray();
        }

        /// <summary>
        /// Returns the plan file name for the given library and plan
        /// </summary>
        /// <param name="lib">The library that the plan is from</param>
        /// <param name="plan">The name of the plan (without the .lap ending)</param>
        /// <returns>The filename with full path of the plan</returns>
        public string getPlanFile(string lib, string plan)
        {
            string planPath=getPlanPath(lib);
            string [] plans={};
            string result="";

            try{
                if (File.Exists(planPath))
                    plans=Directory.GetFiles(planPath,"*",SearchOption.AllDirectories);
                
                foreach (string p in plans)
                {
                    if (p.StartsWith(plan)){
                        result=getPlanPath(lib)+Path.PathSeparator+p;
                        break;
                    }
                }
            } 
            catch (IOException)
            {
                // TODO: @swen: some clever log or comment here!!!
            }
            return result;
        }

        /// <summary>
        /// Returns the file name for the given library (not for plans, see above)
        /// </summary>
        /// <param name="lib">The library that the file is in</param>
        /// <param name="file">The name of the file (including any needed ending)</param>
        /// <returns>The filename with full path</returns>
        string getLibraryFile(string lib, string file)
        {
            return getLibraryPath(lib)+Path.PathSeparator+file;
        }

        /// <summary>
        /// Returns if the given plan of the given library exists.
        /// 
        /// This method only checks if the plan file exists, not if its syntax is
        /// correct.
        /// </summary>
        /// <param name="lib">The library that the plan is from</param>
        /// <param name="plan">The name of the plan (without the .lap ending)</param>
        /// <returns>If the plan exists (i.e. is a file)</returns>
        bool isPlan(string lib,string plan)
        {
            return getPlans(lib).Contains<string>(plan) ? true : false;
        }

        /// <summary>
        /// Returns a list of available behaviour libraries.
        /// 
        /// The function assumes that any directory in the libraries path that
        /// does not start with an '.' is a behaviour library.
        /// </summary>
        /// <returns>List of libraries</returns>
        string [] getLibraries()
        {
            string libraryPath=getLibraryPath();
            string []dirList=Directory.GetDirectories(libraryPath);
            List<string> result=new List<string>();

            foreach(string dir in dirList)
                if (!dir.StartsWith("."))
                    result.Add(dir);


            return result.ToArray();
        }

        /// <summary>
        /// Returns if the given library is a valid behaviour library.
        /// 
        /// This is the case if the following requirements are met:
        ///     - there is a directory with the given name in LIBRARYPATH
        ///     - the directory can be loaded as a module, which requires it to at least
        ///       contain a __init__.py file
        /// </summary>
        /// <param name="lib">Name of the library to check</param>
        /// <returns>If the library is a valid library</returns>
        bool isLibrary(string lib)
        {
            return (Directory.Exists(getLibraryPath(lib))) ? true : false;
        }

        /// <summary>
        /// Returns the default world initialisation script for the given library.
        /// 
        /// If no script was found, then an empty string is returned.
        /// </summary>
        /// <param name="lib">Name of the library</param>
        /// <returns>World initialisatoin script filename, of '' if not found</returns>
        string defaultWorldScript(string lib)
        {
            return File.Exists(getLibraryPath(lib)+Path.PathSeparator+this.worldScript) ? 
                getLibraryPath(lib)+Path.PathSeparator+this.worldScript : "";
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
        public Dictionary<string,List<Type>> getBehaviours(string lib, ILog log=null)
        {
            // get list of python files is behaviour library
            if (log is ILog)
                log.Debug("Scanning library "+lib+" for behaviour classes");
            string libraryPath = getLibraryPath(lib);
            string []files=Directory.GetFiles(libraryPath,"*.dll",SearchOption.TopDirectoryOnly);
            
            Dictionary<string,List<Type>> modules=new Dictionary<string,List<Type>>();
 
            foreach (string f in files)
            {
                Assembly a = Assembly.LoadFile(libraryPath+Path.PathSeparator+f);
                foreach(Type t in a.GetTypes())
                    if (t.IsClass && t.IsSubclassOf(typeof(POSH_sharp.sys.Behaviour)) && t.Name != this.worldScript.Second)
                        if (!modules.ContainsKey(a.FullName))
                            modules.Add(a.FullName,new List<Type> {t});
                        else
                            modules[a.FullName].Add(t);
            }

            return ( modules.Count > 0 ) ? modules : null;
        }

        /// <summary>
        /// Returns the default agent initialisation file filename for the given
        /// library.
        /// 
        /// If no such file exists, and emtpy string is returned.
        /// </summary>
        /// <param name="lib">Name of the library</param>
        /// <returns>Agent initialisation file filename, or "" if not found</returns>
        string defaultAgentInit(string lib)
        {
            string agentInitScript = getLibraryPath(lib)+Path.PathSeparator+agentInit;
            return File.Exists(agentInitScript) ? agentInitScript : "";
        }

        /// <summary>
        /// Returns a unique agent id string of the form 'Axx', where xx is
        /// an increasing number, starting from 00.
        /// 
        /// If more than 99 agents are created, the string length adjusts to the
        /// length of the number.
        /// </summary>
        /// <returns>Unique agent id</returns>
        public string uniqueAgendId()
        {
            return (agentId++).ToString();
        }

        /// <summary>
        /// Runs the given file to initialise the world and returns the world object
        /// and if the world initialisation script creates and runs the agents
        /// itself.
        /// 
        /// This method creates an instance of the class L{World} and makes it
        /// accessible to the world initialisation script under the instance name
        /// 'world'. Using this instance, the world intialisation script can
        /// cimmunicate with this function.
        /// </summary>
        /// <param name="scriptFile">The filename of the script to run</param>
        /// <param name="lib">name of the behaviour library to use by agents</param>
        /// <param name="worldArgs">arguments given to the world initialisation script</param>
        /// <param name="agentsInit">agent initialisation information structure</param>
        /// <returns>tuple (world object, if script created and ran agents)</returns>
        Tuple<World,bool> runWorldScript(Tuple<string,string> scriptFile, string lib, string[] worldArgs = null , Dictionary<string, object> agentsInit = null)
        {
            // TODO: @swen: agentsInit is a file containing environment variables possible switch to json or some other xml notation
            // @raise IOError: If it cannot file the script
            // @raise Exception: If the script causes an exception

            this.world = new World(lib, worldArgs, agentsInit);
            // OLDCOMMENT world instance is given as local variable
            // note that if the global and local variables (when calling execfile)
            // refer to a different object, then some strange things happen as soon as
            // the script starts importing other modules (don't know why). So both
            // arguments have to refer to the same object!
            
            // TODO: this script is not mentioned until now so I need to check what it does and replace its functionality
            // variables = {'world' : w}
            // execfile(script_file, variables, variables)
            
            if (File.Exists(getLibraryPath(lib)+Path.PathSeparator+scriptFile.First))
            {
                // UNCKECKED: this is really complicated to call external Methods from Objects it will need to be checked
                // the idea was to have an external dll including the world setting and load this
                Tuple<Type,object> worldInit = externalDllObjectCall(getLibraryPath(lib)+Path.PathSeparator+scriptFile.First,
                    scriptFile.Second,new object[] {world});
                

                MethodInfo worldInitReturn = worldInit.First.GetMethod("returnWorld");
                
                world = (World) worldInitReturn.Invoke(worldInit.Second,null);

            } else return null;

            return new Tuple<World,bool>(this.world, this.world.createsAgents);
        }


        public static Tuple<Type,object> externalDllObjectCall(string path,string type, object [] constructorParams = null)
        {
            Assembly a = Assembly.LoadFile(path);
            Type typeofObject = a.GetType(type);
            Type[] parameters=null;
            
            if(constructorParams != null)
            {
                parameters=new Type[constructorParams.Length];
                for (int i = 0; i < constructorParams.Length;i++)
                    parameters[i] = constructorParams[i].GetType();
            }

            ConstructorInfo cons = typeofObject.GetConstructor(parameters);
            
            object resultObject = cons.Invoke(constructorParams);

            return new Tuple<Type,object>(typeofObject,resultObject);
        }
        // TODO: @swen currently the profiler does nothing because I removed mason, I shoudl think about including another framework maybe
        // possible options would be netlogo,repast 
        // There must be a better way (& place) to do this...
        // Note, because jython (or maybe mason.jar) is currently 1.4, we need to be sure to compile our classes in 1.4 too
          //  def compile_mason_java():
          //import os
          //ext1='.java'
          //ext2='.class'
          //dir = os.path.join(get_root_path(), config.MASONPATH)
          //dir=get_root_path()+'/platform_files/MASON/'
          //java_src=filter((lambda str:str!="__init__"),map((lambda str:str[:len(str)-len(ext1)]),filter((lambda str: str.endswith(ext1)),os.listdir(dir))))
          //classes=filter((lambda str:str!="__init__$py"),map((lambda str:str[:len(str)-len(ext2)]),filter((lambda str:str.endswith(ext2)),os.listdir(dir))))
   
          //if java_src!=classes:
          //      cmd = 'javac -target 1.4 -source 1.4 -cp %smason.jar %s*.java' % (dir,dir)
          //      os.system(cmd)
    }




}
