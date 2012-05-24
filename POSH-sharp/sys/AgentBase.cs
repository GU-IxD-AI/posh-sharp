using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys
{
    /// <summary>
    /// Base class for POSH agent.
    /// 
    /// This class is not to be instantiated directly. Instead, the strict or
    /// scheduled children should be used.
    /// </summary>
    class AgentBase : LogBase
    {
        public string id {get; private set;}
        public Random random {get; private set;}
        public string library {get; private set;}
        public string world {get; private set;}

        
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
        public AgentBase(string library, string plan, string[] attributes, string world = null) : base("")
        {
        
        // get unique id for agent first, as constructor of LogBase accesses it
        id = unique_agent_id();
        // store library for use when spawning new agents
        this.library = library;
        this.world = world;

        // we need to set the random number generator before we
        // load the behaviours, as they might access it upon
        // construction
        this.random = new Random();

        // if you are profiling, you need to fix this in your init_world.  see library/latchTest for an example & documentation
        // do this before loading Behaviours
        self.profiler=Profiler.initProfile(self) 
        # load and register the behaviours, and reflect back onto agent
        self._bdict = self._loadBehaviours()
        self._reflectBehaviours()
        # more for the profiler
        # FIXME: PR -- is there another place to do this?  will it succeed without MASON? JJB 1 March 2008
        try:
            other.profiler.set_second_name(other._bdict._behaviours['MASON'].name)
        except:
            # may want this for debugging:  print "profiler is off and/or MASON is not being used"
            pass # normally don't expect profiling, nor necessarily MASON
 
        # assign the initial attributes to the behaviours
        self.assignAttributes(attributes)
        # load the plan
        self._loadPlan(get_plan_file(library, plan))
        # loop thread control
        self._exec_loop = False
        self._loop_pause = False
        }
    }
    /**

  
    def getWorld(self):
        """Returns the world object.
        
        The returned world object is the one that was given to the agent
        on construction.
        
        @return: The world object.
        @rtype: unknown
        """
        return self._world

    def getBehaviourDict(self):
        """Returns the agent's behaviour dictionary.

        @return: The agent's behaviour dictionary.
        @rtype: L{POSH.BehaviourDict}
        """
        return self._bdict
    
    def getBehaviours(self):
        """Returns the agent's behaviour objects.
        
        @return: List of behaviour objects.
        @rtype: Sequence of L{POSH.Behaviour}
        """
        return self._bdict.getBehaviours()
    
    def getBehaviour(self, behav_name):
        """Returns the agent's behaviour object with the given name.

        @param behav_name: The name of the behaviour.
        @type behav_name: string
        """
        return self._bdict.getBehaviour(behav_name)
    
    def assignAttributes(self, attributes):
        """Assigns the given attributes to the behaviours.
        
        The attributes are given as a dictionary that maps the tuple
        (behaviour_name, attribute_name) into the assigned values.
        If a behaviour with the given name is not found, an error is raised.
        
        @param attributes: attributes that are to be assigned to behaviours
        @type attributes: dictionary (behaviour_name, attribute_name) -> value
        @raise NameError: If behaviour with given name cannot be found.
        """
        # sort attributes by behaviour
        by_behaviour = {}
        for names, value in attributes.items():
            by_behaviour.setdefault(names[0], {})[names[1]] = value
        # assign attributes to behvaiours
        for behaviour_name, behaviour_attrs in by_behaviour.items():
            self.log.debug("Assigning attributes to behaviour '%s'" % \
                           behaviour_name)
            try:
                self._bdict.getBehaviour( \
                    behaviour_name).assignAttributes(behaviour_attrs)
            except NameError, e:
                raise NameError, "Error assigning attributes to behaviour " \
                                 "'%s': behaviour not found" % behaviour_name
    
    def setRNG(self, rng):
        """Sets the random number generator of the agent.
        
        Whenever the random number generator is set using
        this method, it is probabgated to all registered
        behaviours.
        
        @param rng: A random number generator.
        @type rng: Similar to the python 'random' module
        """
        self.random = rng
        for behaviour in self._bdict.getBehaviours():
            behaviour.setRNG(rng)
    
    def reset(self, waittime = 300):
        """Resets the agent. Should be called just before running the main loop.
        
        The method returns if the reset was successful. It first checks if
        there are any behaviours to reset, and then resets all of them. It then
        waits until each of them is ready, by calling checkError(). If any of
        these fail, then the method returns False, otherwise it returns True.
        
        The waittime is the time allowed until the behaviours are getting
        ready. It is the same as given to checkError(). By default, it is
        set to 20 seconds.
        
        @param waittime: Timout waiting for behaviours (see L{checkError()}).
        @type waittime: int
        @return: If the reset was successful.
        @rtype: bool
        """
        self.log.debug("Resetting the behaviours")
        if len(self._bdict.getBehaviours()) == 0:
            self.log.error("No behaviours registered")
            return False
        # reset behaviours one by one
        for behaviour in self._bdict.getBehaviours():
            self.log.debug("Resetting behaviour '%s'" % behaviour.getName())
            if not behaviour.reset():
                self.log.error("Resetting behaviour '%s' failed" % \
                               behaviour.getName())
                return False
        # check if they are ready
        if self.checkError(waittime) > 0:
            return False
        self.log.debug("Reset successful")
        return True
    
    def checkError(self, waittime = 0):
        """Returns the number of behaviours that report an error.
        
        This method checks the state of the behaviours, and returns True if
        they are ready, and False otherwise. The method waits a maximum time,
        specified by 'waittime' for the behaviours to become ready. The
        state of a behaviour is checked by calling its checkError() methods.
        
        'waittime' is specified in 10ths of a second. If waittime = 20, for
        example, then the method waits at most 2 seconds. A waittime of 0
        requires the behaviours to be available immediately.
        
        @param waittime: Number of 10ths of seconds to wait for behaviours
            to be ready.
        @type waittime: int
        @return: Number of behaviours that are not ready.
        @rtype: int
        """
        self.log.debug("Waiting for behaviours ready")
        def count_errors():
            error_count = 0
            for behaviour in self._bdict.getBehaviours():
                if behaviour.checkError():
                    error_count += 1
            return error_count
        count = count_errors()
        while count > 0 and waittime > 0:
            time.sleep(0.1)
            waittime -= 1
            count = count_errors()
        if count > 0:
            self.log.error("Waiting for behaviours ready timed out")
        else:
            self.log.debug("Behaviours ready")
        return count
    
    def startLoop(self):
        """Start the real-time loop, repeatedly firing the drive collection.

        This method resets the agent by calling L{reset()} and then
        spawns a separate thread (L{_loop_thread}) that repeateadly
        fires the drive collection until the drive collection
        fails, the goal is reached, or L{stopLoop} is called.

        @return: True if the loop was started, and False if it is already
            running, or the reset failed.
        @rtype: bool
        """
        # don't start the loop twice
        if not self.reset() or self._exec_loop:
            return False
        self.log.debug("Starting real-time loop")
        self._loop_pause = False
        self._exec_loop = True
        thread.start_new_thread(self._loop_thread_wrapper, ())
        return True

    def pauseLoop(self):
        """Pauses the real-time loop, or continues it.

        If this method is called when the real-time loop is not
        paused, then it pauses it and returns True. If the real-time
        loop is paused, then the loop is continued, and False is
        returned. If the real-time loop is not running, then an
        exception is thrown.

        @return: True if loop is paused, and False if it is continued.
        @rtype: bool.
        @raise Exception: If called when real-time loop is not activated.
        """
        if not self._exec_loop:
            raise Exception, "pauseLoop() called while real-time loop was " \
                "not running"
        self._loop_pause = not self._loop_pause
        if self._loop_pause:
            self.log.debug("Pausing real-time loop")
        else:
            self.log.debug("Continuing real-time loop")
        return self._loop_pause

    def stopLoop(self):
        """Stops the real-time loop.

        This method can also be called when the loop is paused. If it is
        called when the loop is not running, an exception is thrown.

        @raise Exception: If called when real-time loop is not activated.
        """
        if not self._exec_loop:
            raise Exception, "stopLoop() called while real-time loop was " \
                "not running"
        self._exec_loop = False
        self._loop_pause = False
        self.log.debug("Real-time loop stopped")
    
    def loopStatus(self):
        """Returns the status of the real-time loop.
        
        The state is returned as the tuple (isRunning, isPaused). isRunning
        is only true if the loop is currently executed. If this is the case,
        then isPaused indicates if the loop is currently paused.
        
        @return: status of the real-time loop.
        @rtype: (bool, bool)
        """
        return (self._exec_loop, self._loop_pause)
    
    def exitPrepare(self):
        """Prepares the agent to exit.
        
        This method stops the loop if it is running and calls exitPrepare() of
        all behaviours.
        """
        self.log.info("Exit Command Received - Prepare to exit")
        if self._exec_loop:
            self.stopLoop()
        for behaviour in self._bdict.getBehaviours():
            behaviour.exitPrepare()

    def followDrive(self):
        """Performes one loop through the drive collection.
        
        This method needs to be overriden by inheriting classes.
        
        @raise NotImplementedError: always.
        """
        raise NotImplementedError, \
              "AgentBase.followDrive() needs to be overridden"

    def _loadBehaviours(self):
        """Returns all behaviours of the agent's library as a behaviour
        dictionary.
        
        @return: Behaviour dictionary with all behaviours in the library
        @rtype: L{POSH.BehaviourDict}
        """
        bdict = BehaviourDict()
        self.log.info("Scanning library for behaviours")
        behaviour_classes = get_behaviours(self._library, self.log)
        for behaviour_class in behaviour_classes:
            self.log.info("Creating instance of behaviour '%s'" % \
                          behaviour_class.__name__)
            if (self.profiler is not None):
                try:
                    behaviour = behaviour_class(self,self.profiler)
                except TypeError: 
                    behaviour = behaviour_class(self)            
            else:
                behaviour = behaviour_class(self)            
            
            self.log.debug("Registering behaviour in behaviour dictionary")
            bdict.registerBehaviour(behaviour)
        return bdict
    
    def _reflectBehaviours(self):
        """Reflect the agent objects onto the agent.
        
        This action is performed for all behaviour objects in the agent's
        behaviour dictionary. If an attribute with the same name already
        exists, an error is raised.
        
        @raise AttributeError: If behaviour name clashes with already existing
            agent attributes.
        """
        for behaviour in self._bdict.getBehaviours():
            behaviour_name = behaviour.getName()
            self.log.debug("Reflecting behaviour '%s' back onto agent" % \
                           behaviour_name)
            if hasattr(self, behaviour_name):
                raise AttributeError, \
                  "Assigning behaviour '%s' failed due to name clash with existing attribute" % \
                  behaviour_name
            setattr(self, behaviour_name, behaviour)
    
    def _loadPlan(self, planfile):
        """Needs to be overridden to load the plan and create the data
        structures.
        
        It is called form the constructor after the behaviours have been
        loaded and their initial attributes have been assigned.
        
        @param planfile: filename of the plan file to load
        @type planfile: string
        """
        raise NotImplementedError, \
              "AgentBase._loadPlan() needs to be overridden"
    
    def _loop_thread_wrapper(self):
        """A wrapper for the _loop_thread() method.
        
        If calles _loop_thread(), and sets the object variables
        _exec_loop and _loop_pause to the correct values after
        _loop_thread() returns.
        """
        self._loop_thread()
        self._exec_loop = False
        self._loop_pause = False
    
    def _loop_thread(self):
        """The loop thread, started by L{startLoop}.

        This method needs to be overridden by inheriting classes.
        If needs to check the status of the object variables _exec_loop and
        _loop_pause and react to them.
        
        @raise NotImplementedError: always
        """
        raise NotImplementedError, \
              "AgentBase._loop_thread() needs to be overridden"
     * */
}
