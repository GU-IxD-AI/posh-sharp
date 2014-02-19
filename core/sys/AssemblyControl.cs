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
    public class AssemblyControl
    {
        int agentId = 0;
        
        static AssemblyControl instance;

        public static AssemblyControl GetControl()
        {
            if (instance is AssemblyControl || ( instance != null && instance.GetType().IsSubclassOf(typeof(AssemblyControl))))
                return instance;
            else
            {
                Environment.SetEnvironmentVariable("POSHUnityMode", "False");
                instance = new AssemblyControl();
                return instance;
            }

        }

        public static void SetForUnityMode()
        {
            Environment.SetEnvironmentVariable("POSHUnityMode","True");

            if (!instance.GetType().IsSubclassOf(typeof(AssemblyControl)))
                instance = new MobileControl();
        }


        protected AssemblyControl()
        {
            config = new Dictionary<string,string> {
                {"InitPath","init"},
                {"PlanPath","plans"},
                {"LibraryPath","library"},
                {"PlanEnding",".lap"}
            };
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
        public Dictionary<string,string> config{get; set;}




        /// <summary>
        /// Returns the root path that POSH is installed in. Assumes that this
        /// module resides in the root path.
        /// </summary>
        /// <returns>Root path</returns>
        public string getRootPath()
        {
            string assembly = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            return assembly;
        }

		private string getAssemblyLibrary()
		{
			return getAssemblyLibrary ("");
		}

        /// <summary>
        /// Returns the path to the behaviour assembly directory. If no library is given,
        /// then the base path of the library is returned.
        /// </summary>
        /// <param name="lib">The library to return the path for</param>
        /// <returns>The base library path or the path to the given library</returns>
        private string getAssemblyLibrary(string lib)
        {
            string path = getRootPath() + Path.DirectorySeparatorChar + config["LibraryPath"];
            if (!Directory.Exists(path))
                return null;
            if (lib.Split(',').Length > 1)
                lib = lib.Split(',')[0];

            return (lib != string.Empty && Directory.Exists(path + Path.DirectorySeparatorChar + lib)) ? path + Path.DirectorySeparatorChar + lib : path;
        }

        /// <summary>
        /// Returns the path to the plans of the given behaviour library.
        /// </summary>
        /// <param name="lib">The library to return the path for</param>
        /// <returns>The path to the plans</returns>
        private string getPlanPath(string lib)
        {
            return getAssemblyLibrary(lib)+Path.DirectorySeparatorChar+config["PlanPath"];
            
        }

        /// <summary>
        /// Returns if the given class object is a subclass of a behaviour class.
        /// </summary>
        /// <param name="o">An object</param>
        /// <returns>If the given class is a subclass of L{POSH.Behaviour}</returns>
        internal bool isBehaviour(object o)
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
        private string [] getPlans(string lib)
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
        internal Stream GetPlanFile(string lib, string plan)
        {
            string planPath=getPlanPath(lib);
            string [] plans={};
            string result="";

            try{
                if (Directory.Exists(planPath))
                    plans=Directory.GetFiles(planPath,"*",SearchOption.AllDirectories);
                
                foreach (string p in plans)
                {
                    if (p.Split(Path.DirectorySeparatorChar)
                            .Last().Contains(plan))
                    {
                        result=p;
                        break;
                    }
                }
            } 
            catch (IOException)
            {
                // TODO: @swen: some clever log or comment here!!!
            }
            Stream planStream = new StreamReader(File.OpenRead(result)).BaseStream;
            
            return planStream;
        }

        /// <summary>
        /// Returns the file name for the given library (not for plans, see above)
        /// </summary>
        /// <param name="lib">The library that the file is in</param>
        /// <param name="file">The name of the file (including any needed ending)</param>
        /// <returns>The filename with full path</returns>
        StreamReader getLibraryFile(string lib, string file)
        {
            return new StreamReader(getAssemblyLibrary(lib)+Path.DirectorySeparatorChar+file);
        }

        /// <summary>
        /// Returns the file name for the given library (not for plans, see above)
        /// </summary>
        /// <param name="lib">The library that the file is in</param>
        /// <param name="file">The name of the file (including any needed ending)</param>
        /// <returns>The filename with full path</returns>
        string getAssemblyFile(string lib, string file)
        {
            return getAssemblyLibrary(lib) + Path.DirectorySeparatorChar + file;
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
        public bool isPlan(string lib,string plan)
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
        internal string [] getLibraries()
        {
            string libraryPath=getAssemblyLibrary();
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
        public bool isLibrary(string assembly,string lib)
        {
            if (!File.Exists(getAssemblyLibrary() + Path.DirectorySeparatorChar + assembly))
                return false;

            Assembly libAssembly = Assembly.LoadFile(getAssemblyLibrary() + Path.DirectorySeparatorChar + assembly);
            ManifestResourceInfo info = libAssembly.GetManifestResourceInfo(lib);
            
            return (File.Exists(getAssemblyLibrary(lib))) ? true : false;
        }

        /// <summary>
        /// Returns the default world initialisation script for the given library.
        /// 
        /// If no script was found, then an empty string is returned.
        /// </summary>
        /// <param name="lib">Name of the library</param>
        /// <returns>World initialisatoin script filename, of '' if not found</returns>
        public string defaultWorldScript(string lib)
        {
            return File.Exists(getAssemblyLibrary(lib)+Path.DirectorySeparatorChar+this.worldScript) ? 
                getAssemblyLibrary(lib)+Path.DirectorySeparatorChar+this.worldScript : "";
        }

        public virtual BehaviourDict GetBehaviours(string lib, AgentBase agent)
		{
			return GetBehaviours (lib, null, agent);
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
        public virtual BehaviourDict GetBehaviours(string lib, ILog log,AgentBase agent)
        {
            BehaviourDict dict = new BehaviourDict();
            Type[] types = new Type[1] { typeof(AgentBase) };

            if (log is ILog)
                log.Debug("Scanning library "+lib+" for behaviour classes");

            
            Assembly a = GetAssembly(lib);
            foreach(Type t in a.GetTypes())
                if (t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(POSH_sharp.sys.Behaviour)) && (this.worldScript == null || t.Name != this.worldScript.Second))
                {
                    log.Info(String.Format("Creating instance of behaviour {0}.", t));
                    ConstructorInfo behaviourConstruct = t.GetConstructor(types);
                    object[] para = new object[1] { agent };
                    log.Debug("Registering behaviour in behaviour dictionary");
                    if (behaviourConstruct != null)
                        dict.RegisterBehaviour((Behaviour)behaviourConstruct.Invoke(para));
                }
            
            return ( dict.getBehaviours().Count() > 0 ) ? dict : new BehaviourDict();
        }

        /// <summary>
        /// Returns the default agent initialisation file filename for the given
        /// library.
        /// 
        /// If no such file exists, and emtpy string is returned.
        /// </summary>
        /// <param name="lib">Name of the library</param>
        /// <returns>Agent initialisation file filename, or "" if not found</returns>
        public string defaultAgentInit(string lib)
        {
            string agentInitScript = getAssemblyLibrary(lib)+Path.DirectorySeparatorChar+config["InitPath"]+lib+"_init.txt";
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
        public string UniqueAgendId()
        {
            return (agentId++).ToString();
        }
		public Tuple<World, bool> runWorldScript(Type worldType, string assembly)
		{
			return runWorldScript(worldType, assembly,null, null);
		}
        // TODO: Check as this is new and untested
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
        public Tuple<World, bool> runWorldScript(Type worldType, string assembly, string worldArgs, List<Tuple<string, object>> agentsInit)
        {
            // TODO: @swen: agentsInit is a file containing environment variables possible switch to json or some other xml notation
            // @raise IOError: If it cannot file the script
            // @raise Exception: If the script causes an exception

            // this.world = new World(lib, worldArgs, agentsInit);
            // OLDCOMMENT world instance is given as local variable
            // note that if the global and local variables (when calling execfile)
            // refer to a different object, then some strange things happen as soon as
            // the script starts importing other modules (don't know why). So both
            // arguments have to refer to the same object!
            
            // TODO: this script is not mentioned until now so I need to check what it does and replace its functionality
            // variables = {'world' : w}
            // execfile(script_file, variables, variables)
            
            if (worldType.IsClass && worldType.IsSubclassOf(typeof(World)))
            {
                // UNCKECKED: this is really complicated to call external Methods from Objects it will need to be checked
                // the idea was to have an external dll including the world setting and load this
                World world = (World)ClassConstructorCall(worldType,new object[] {assembly, worldArgs,agentsInit});
                
            } else return null;

            return new Tuple<World,bool>(this.world, this.world.createsAgents);
        }

		public static object ClassConstructorCall(Type worldType)
		{
			return ClassConstructorCall (worldType, null);
		}

        public static object ClassConstructorCall(Type worldType, object [] constructorParams)
        {
            Type[] parameters=null;
            
            if(constructorParams != null)
            {
                parameters=new Type[constructorParams.Length];
                for (int i = 0; i < constructorParams.Length;i++)
                    parameters[i] = constructorParams[i].GetType();
            }

            ConstructorInfo cons = worldType.GetConstructor(parameters);
            
            object resultObject = cons.Invoke(constructorParams);

            return resultObject;
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

        public bool checkDirectory(string planDir)
        {
            return Directory.Exists(getAssemblyLibrary() + Path.DirectorySeparatorChar + planDir);
        }

        public bool IsAssembly(string assembly)
        {
            if (GetAssembly(assembly) is Assembly)
                return true;

            return false;
            
        }

        public Assembly GetAssembly(string assembly)
        {
            if (!File.Exists(getAssemblyLibrary() + Path.DirectorySeparatorChar+ assembly))
                return null;
            try
            {
                Assembly libAssembly = Assembly.LoadFile(getAssemblyLibrary() + Path.DirectorySeparatorChar + assembly);
                return libAssembly;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine(string.Format("The Assembly can not be found inside '{0}' directory", getRootPath()));
            }
            catch (FileLoadException)
            {
                Console.WriteLine(string.Format("A file was found inside '{0}' directory named '{1}' but is not an assembly", getRootPath(), assembly));
            }
            catch (BadImageFormatException)
            {
                Console.WriteLine(string.Format("An assembly named '{0}' was found but is malformatted (possibly wrong system or architecture).", assembly));
            }
            return null;

        }

        public bool IsLibraryInAssembly(string assembly,string agentLibrary)
        {
            Module library = null;
            if (!IsAssembly(assembly))
                return false;

            Assembly libAssembly = Assembly.LoadFile(getAssemblyLibrary() + Path.DirectorySeparatorChar + assembly);
            
            try
            {
                // TODO: include cases for other platforms
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        agentLibrary += ".dll";
                        break;
                    default:
                        break;
                }
                library = libAssembly.GetModule(agentLibrary);
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine(string.Format("The Assembly can not be found inside '{0}' directory",getRootPath()));
            }
            catch (FileLoadException) 
            {
                Console.WriteLine(string.Format("A file was found inside '{0}' directory named '{1}' but is not an assembly",getRootPath(),assembly));
            }
            catch(BadImageFormatException)
            {
                Console.WriteLine(string.Format("An assembly named '{0}' was found but is malformatted (possibly wrong system or architecture).",assembly));
            }

            return (library  != null ) ? true : false;
        }

        public bool CheckAgentInitFile(string agentsInitFile)
        {
            if (this.config["InitPath"] == null || !Directory.Exists(getAssemblyLibrary()+Path.DirectorySeparatorChar+this.config["InitPath"]))
                return false;
            if (File.Exists(getAssemblyLibrary() + Path.DirectorySeparatorChar + this.config["InitPath"] + Path.DirectorySeparatorChar + agentsInitFile))
                return true;
            
            return false;
        }

        public virtual StreamReader GetAgentInitFileStream(string agentsInitFile)
        {
            if (!CheckAgentInitFile(agentsInitFile))
                return null;
            
            return File.OpenText(getAssemblyLibrary() + Path.DirectorySeparatorChar + this.config["InitPath"] + Path.DirectorySeparatorChar + agentsInitFile);
            
        }

        public virtual StreamReader GetAgentInitFileStream(string assembly, string agentsInitFile)
        {
            Assembly assem = GetAssembly(assembly);
            if (assem is Assembly)
                foreach (String name in assem.GetManifestResourceNames())
                {
                    if (name == agentsInitFile)
                    {
                        return new StreamReader(assem.GetFile(name));
                    }

                }



            return null;
        }
    }




}
