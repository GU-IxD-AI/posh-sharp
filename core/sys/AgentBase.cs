using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using POSH_sharp.sys.strict;
using System.Threading;
using System.IO;
using POSH_sharp.sys.exceptions;

namespace POSH_sharp.sys
{
    /// <summary>
    /// Base class for POSH agent.
    /// 
    /// This class is not to be instantiated directly. Instead, the strict or
    /// scheduled children should be used.
    /// </summary>
    public class AgentBase : LogBase
    {
        public string id {get; private set;}
        public Random random {get; private set;}
        public string library {get; private set;}
        public World world {get; private set;}
        public Profiler profiler {get; set;}
        
        private BehaviourDict _bdict;

        private Thread myThread;
        protected internal bool _loopPause;
        protected internal bool _execLoop;

        /// <summary>
        /// Initialises the agent to use the given library and plan.
        /// 
        /// The plan has to be given as the plan name without the '.lap' extension.
        /// The attributes are the ones that are assigned to the behaviours
        /// when they are initialised. The world is the one that can be accessed
        /// by the behaviours by the L{AgentBase.getWorld} method.
        /// 
        /// Note that when the behaviours are loaded from the given library, then
        /// they are reflected onto the agent object. That means, given that
        /// there is a behaviour called 'bot', then it can be accessed from another
        /// behaviour either by self.agent.getBehaviour("bot"), or by
        /// self.agent.bot. Consequently, behaviour names that clash with already
        /// existing agent attributes cause an AttributeError to be raise upon
        /// initialising the behaviours.
        /// 
        /// The attributes are to be given in the same format as for the
        /// method L{AgentBase.assignAttributes}.
        /// </summary>
        /// <param name="library">The behaviour library to use.</param>
        /// <param name="plan">The plan to use (without the '.lap' ending).</param>
        /// <param name="attributes">The attributes to be assigned to the behaviours</param>
        /// <param name="world"></param>
        public AgentBase(string library, string plan, Dictionary<Tuple<string,string>,object> attributes, World world = null) 
            : base("")
        {
            // get unique id for agent first, as constructor of LogBase accesses it
            id = WorldControl.GetControl().uniqueAgendId();
            // store library for use when spawning new agents
            this.library = library;
            this.world = world;

            // we need to set the random number generator before we
            // load the behaviours, as they might access it upon
            // construction
            this.random = new Random();

            // if you are profiling, you need to fix this in your init_world.  see library/latchTest for an example & documentation
            // do this before loading Behaviours
            profiler= Profiler.initProfile(this); 
            // load and register the behaviours, and reflect back onto agent
            this._bdict = loadBehaviours();
            reflectBehaviours();

            //# more for the profiler
            // FIXME: PR -- is there another place to do this?  will it succeed without MASON? JJB 1 March 2008
            //try:
            //    other.profiler.set_second_name(other._bdict._behaviours['MASON'].name)
            //except:
            //    # may want this for debugging:  print "profiler is off and/or MASON is not being used"
            //    pass # normally don't expect profiling, nor necessarily MASON
 
            // assign the initial attributes to the behaviours
            this.assignAttributes(attributes);
            
            // load the plan
            loadPlan(WorldControl.GetControl().getPlanFile(library, plan));
            // loop thread control
            _execLoop = false;
            _loopPause = false;
        }

        /// <summary>
        /// Returns all behaviours of the agent's library as a behaviour
        /// dictionary.
        /// </summary>
        /// <returns>Behaviour dictionary with all behaviours in the library</returns>
        internal BehaviourDict loadBehaviours()
        {
            BehaviourDict dict = new BehaviourDict();
            log.Info("Scanning library for behaviours");
            Dictionary<string,List<Type>> behaviourClasses = WorldControl.GetControl().getBehaviours(library,log);
            Type [] types=new Type[1] {typeof(AgentBase)};
            
            foreach (KeyValuePair<string,List<Type>> assembly in behaviourClasses)
                foreach (Type behaviourClass in assembly.Value)
                {
                    // TODO: Profiler needs to be included later on.
                    //    self.log.info("Creating instance of behaviour '%s'" % \
                    //                  behaviour_class.__name__)
                    //    if (self.profiler is not None):
                    //        try:
                    //            behaviour = behaviour_class(self,self.profiler)
                    //        except TypeError: 
                    //            behaviour = behaviour_class(self)            
                    //    else:
                    //        behaviour = behaviour_class(self)  
                    log.Info(String.Format("Creating instance of behaviour {0}.",behaviourClass));
                    ConstructorInfo behaviourConstruct = behaviourClass.GetConstructor(types);
                    object[] para= new object[1] {this};
                    log.Debug("Registering behaviour in behaviour dictionary");
                    _bdict.RegisterBehaviour((Behaviour) behaviourConstruct.Invoke(para));
                }
            
            return null;
        }

        /// <summary>
        /// Reflect the agent objects onto the agent.
        /// 
        /// This action is performed for all behaviour objects in the agent's
        /// behaviour dictionary. If an attribute with the same name already
        /// exists, an error is raised.
        /// </summary>
        /// 
        internal void reflectBehaviours()
        {
            foreach (Behaviour behave in _bdict.getBehaviours())
            {
                log.Debug(String.Format("Reflecting behaviour {0} back onto agent" ,
                           behave.GetName()));
                if (this.attributes.ContainsKey(behave.GetName()))
                    throw new AttributeException(string.Format(
                        "Assigning behaviour {0} failed due to name clash with existing attribute",
                        behave.GetName()
                        ));
                this.attributes.Add(behave.GetName(), behave);
            }
        }

        /// <summary>
        /// Returns the world object.
        /// 
        /// The returned world object is the one that was given to the agent
        /// on construction.
        /// </summary>
        /// <returns></returns>
        public World getWorld()
        {
            return world;
        }

        /// <summary>
        /// Returns the agent's behaviour dictionary.
        /// </summary>
        /// <returns>The agent's behaviour dictionary.</returns>
        public BehaviourDict getBehaviourDict()
        {
            return _bdict;
        }

        /// <summary>
        /// Returns the agent's behaviour objects.
        /// </summary>
        /// <returns>List of behaviour objects.</returns>
        public Behaviour[] getBehaviours()
        {
            return _bdict.getBehaviours();
        }

        /// <summary>
        /// Returns the agent's behaviour object with the given name.
        /// </summary>
        /// <param name="behaviourName">The name of the behaviour.</param>
        /// <returns>The behaviour for the supplied name or null.</returns>
        public Behaviour getBehaviour(string behaviourName)
        {
            return _bdict.getBehaviour(behaviourName);
        }

        /// <summary>
        /// Assigns the given attributes to the behaviours.
        /// 
        /// The attributes are given as a dictionary that maps the tuple
        /// (behaviour_name, attribute_name) into the assigned values.
        /// If a behaviour with the given name is not found, an error is raised.
        /// </summary>
        /// <param name="attributes">attributes that are to be assigned to behaviours
        /// dictionary (behaviour_name, attribute_name) -> value</param>
        public void assignAttributes(Dictionary<Tuple<string, string>, object> attributes)
        {
            // # sort attributes by behaviour
            foreach (KeyValuePair<Tuple<string, string>, object> pair in attributes.OrderBy(pair => pair.Key.First))
            {
                // # assign attributes to behaviours
                log.Debug(String.Format("Assigning attributes to behaviour {0}", pair.Key.First));
                try
                {
                    _bdict.getBehaviour(pair.Key.First).AssignAttributes((Dictionary<string, object>)pair.Value);
                }
                catch (NameException )
                {
                    throw new NameException(string.Format(
                        "Error assigning attributes to behaviour {0}: behaviour not found",
                        pair.Key.First));
                }
            }

        }

        /// <summary>
        /// Sets the random number generator of the agent.
        /// 
        /// Whenever the random number generator is set using
        /// this method, it is probabgated to all registered
        /// behaviours.
        /// </summary>
        /// <param name="rng">A random number generator.</param>
        public void setRNG(Random rng)
        {
            this.random = rng;
            foreach (Behaviour behave in _bdict.getBehaviours())
                behave.SetRNG(rng);
        }


        /// <summary>
        /// Resets the agent. Should be called just before running the main loop.
        /// 
        /// The method returns if the reset was successful. It first checks if
        /// there are any behaviours to reset, and then resets all of them. It then
        /// waits until each of them is ready, by calling checkError(). If any of
        /// these fail, then the method returns False, otherwise it returns True.
        /// 
        /// The waittime is the time allowed until the behaviours are getting
        /// ready. It is the same as given to checkError(). By default, it is
        /// set to 20 seconds.
        /// </summary>
        /// <param name="waitTime">Timout waiting for behaviours (see L{checkError()}).</param>
        /// <returns></returns>
        public virtual bool reset(int waitTime = 300)
        {
            log.Debug("Resetting the behaviours");
            if (_bdict.getBehaviours().Length == 0)
            {
            log.Error("No behaviours registered");
            return false;
            }
            // # reset behaviours one by one
            foreach (Behaviour behave in _bdict.getBehaviours())
            {
                log.Debug(String.Format("Resetting behaviour {0}", behave.GetName()));
                if (!behave.Reset())
                {
                    log.Error(String.Format("Resetting behaviour {0} failed", behave.GetName()));
                    return false;
                }
            }
            if (checkError(waitTime) > 0)
                return false;

        log.Debug("Reset successful");
        return true;
        }

        /// <summary>
        /// Returns the number of behaviours that report an error.
        /// 
        /// This method checks the state of the behaviours, and returns True if
        /// they are ready, and False otherwise. The method waits a maximum time,
        /// specified by 'waittime' for the behaviours to become ready. The
        /// state of a behaviour is checked by calling its checkError() methods.
        /// 
        /// 'waittime' is specified in 10ths of a second. If waittime = 20, for
        /// example, then the method waits at most 2 seconds. A waittime of 0
        /// requires the behaviours to be available immediately.
        /// </summary>
        /// <param name="waitTime">Number of 10ths of seconds to wait for behaviours
        /// to be ready.</param>
        /// <returns>Number of behaviours that are not ready.</returns>
        public int checkError(int waitTime = 0)
        {
            log.Debug("Waiting for behaviours ready");
            int error = countErrors();
            while (error > 0 && waitTime > 0)
            {
                Thread.Sleep(10);
                waitTime -= 1;
                error = countErrors();
            }
            if (error > 0)
                log.Error("Waiting for behaviours ready timed out");
            else
                log.Debug("Behaviours ready");

            return error;
        }

        private int countErrors()
        {
            int error = 0;
            foreach (Behaviour behave in _bdict.getBehaviours())
                if (behave.CheckError())
                    error++;

            return error;
        }

        /// <summary>
        /// Start the real-time loop, repeatedly firing the drive collection.
        /// 
        /// This method resets the agent by calling L{reset()} and then
        /// spawns a separate thread (L{_loop_thread}) that repeateadly
        /// fires the drive collection until the drive collection
        /// fails, the goal is reached, or L{stopLoop} is called.
        /// </summary>
        /// <returns>True if the loop was started, and False if it is already
        ///     running, or the reset failed.</returns>
        public bool startLoop()
        {
            //# don't start the loop twice
            if (!reset() || _execLoop)
                return false;
            log.Debug("Starting real-time loop");
            this._loopPause = false;
            this._execLoop = true;

            myThread = new Thread(this.loopThreadWrapper);
            return true;
        }

        /// <summary>
        /// Pauses the real-time loop, or continues it.
        /// 
        /// If this method is called when the real-time loop is not
        /// paused, then it pauses it and returns True. If the real-time
        /// loop is paused, then the loop is continued, and False is
        /// returned. If the real-time loop is not running, then an
        /// exception is thrown.
        /// </summary>
        /// <returns></returns>
        public bool pauseLoop()
        {
            if (!_execLoop)
                throw new ThreadStateException("pauseLoop() called while real-time loop was not running");
            _loopPause = !_loopPause;

            if (_loopPause)
                log.Debug("Pausing real-time loop");
            else
                log.Debug("Continuing real-time loop");

            return _loopPause;
        }

        /// <summary>
        /// Stops the real-time loop.
        /// 
        /// This method can also be called when the loop is paused. If it is
        /// called when the loop is not running, an exception is thrown.
        /// </summary>
        public void stopLoop()
        {
            if (!_execLoop)
                throw new ThreadStateException("stopLoop() called while real-time loop was not running");
            _execLoop = false;
            _loopPause = false;
            log.Debug("Real-time loop stopped");
        }

        /// <summary>
        /// Returns the status of the real-time loop.
        /// 
        /// The state is returned as the tuple (isRunning, isPaused). isRunning
        /// is only true if the loop is currently executed. If this is the case,
        /// then isPaused indicates if the loop is currently paused.
        /// </summary>
        /// <returns>Returns the state of the Tread in the form Tuple(isRunning, isPaused).</returns>
        public Tuple<bool, bool> loopStatus()
        {
            return new Tuple<bool, bool>(_execLoop, _loopPause);
        }

        /// <summary>
        /// Prepares the agent to exit.
        /// 
        /// This method stops the loop if it is running and calls exitPrepare() of
        /// all behaviours.
        /// </summary>
        public void exitPrepare()
        {
            log.Info("Exit Command Received - Prepare to exit");
            if (_execLoop)
                stopLoop();
            foreach (Behaviour behave in _bdict.getBehaviours())
                behave.ExitPrepare();
        }

        /// <summary>
        /// Performes one loop through the drive collection.
        /// 
        /// This method needs to be overriden by inheriting classes.
        /// </summary>
        public virtual int followDrive()
        {
            throw new NotImplementedException("AgentBase.followDrive() needs to be overridden");
        }

        /// <summary>
        /// Needs to be overridden to load the plan and create the data
        /// structures.
        /// 
        /// It is called form the constructor after the behaviours have been
        /// loaded and their initial attributes have been assigned.
        /// </summary>
        /// <param name="planFile"></param>
        /// <returns></returns>
        public  virtual void loadPlan(string planFile)
        {
            throw new NotImplementedException("AgentBase._loadPlan() needs to be overridden");
        }

        /// <summary>
        /// A wrapper for the _loopThread() method.
        /// 
        /// It calles loopThread(), and sets the object variables
        /// _execLoop and _loopPause to the correct values after
        /// loopThread() returns.
        /// </summary>
        private void loopThreadWrapper()
        {
            loopThread();
            _execLoop = false;
            _loopPause = false;

        }

        /// <summary>
        /// The loop thread, started by startLoop.
        /// 
        /// This method needs to be overridden by inheriting classes.
        /// If needs to check the status of the object variables _execLoop and
        /// _loopPause and react to them.
        /// </summary>
        public virtual void loopThread()
        {
            throw new NotImplementedException("AgentBase._loop_thread() needs to be overridden");
        }

    }

}
