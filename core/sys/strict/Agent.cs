using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys;
using System.Threading;
using System.IO;
using POSH_sharp.sys.parse;

namespace POSH_sharp.sys.strict
{
    /// <summary>
    /// Implementation of a POSH Agent.
    /// </summary>
    public class Agent : AgentBase
    {
        // drive collection results
        public const int DRIVEFOLLOWED   =  0;
        public const int DRIVEWON        =  1;
        public const int DRIVELOST       = -1;


        private TimerBase timer;

        protected internal DriveCollection dc;

        public Agent(string library, string plan, Dictionary<Tuple<string,string>,object> attributes, World world = null)
            : base(library,plan,attributes,world)
        {
            
            
           
            // PARAMETER: set the initial loop frequency to 20Hz
            setLoopFreq(1000/20);
        }

        /// <summary>
        /// Sets the agent timer.

        /// The agent timer determines the timing behaviour of an agent.
        /// Is is usually set when loading a plan, as the drive collection
        /// specifies if a stepped timer (DC) or a real-time timer (RDC) is
        /// required.
        /// </summary>
        /// <param name="timer">The agent's timer.</param>
        public void setTimer(TimerBase timer)
        {
            this.timer = timer;
        }

        /// <summary>
        /// Returns the currently used timer.
        /// </summary>
        /// <returns>The currently used timer.</returns>
        public TimerBase getTimer()
        {
            return timer;
        }

        /// <summary>
        /// Sets the loop frequency of real-time plans.

        /// Calling this method sets the loop frequency of real-time plans.
        /// The loop frequency is the frequency at which the main POSH loop
        /// is executed. The given frequency is an upper bound on the real
        /// execution frequency.
        /// </summary>
        /// <param name="freq">The loop frequency, given in milliseconds that
        ///         pass between two calls of the main loop.</param>
        public void setLoopFreq(long freq)
        {
            // causes an AttributeError for non-real-time timers
            timer.SetLoopFreq(freq);
        }

        /// <summary>
        /// Checks if the behaviours are ready and resets the agent's timer.
        /// 
        /// This method should be called just before running the main loop.
        ///
        /// The waittime is the time allowed until the behaviours are getting
        /// ready. It is the same as given to checkError(). By default, it is
        /// set to 20 seconds.
        /// </summary>
        /// <param name="waitTime">Timout waiting for behaviours (see L{checkError()}).</param>
        /// <returns>If the reset was successful.</returns>
        public override bool  reset(int waitTime = 300)
        {
 	         if (!base.reset())
                 return false;
            timer.Reset();
            return true;
        }

        /// <summary>
        /// Performes one loop through the drive collection.
        ///         
        /// This method takes the first triggering drive element and either
        /// descends further down in the competence tree, or performs
        /// the drive's current action.
        /// 
        /// It returns either DRIVE_WON if the drive collection's goal was
        /// reached, DRIVE_LOST if no drive triggered, or DRIVE_FOLLOWED if
        /// the goal wasn't reached and a drive triggered.
        /// </summary>
        /// <returns></returns>
        public override int followDrive()
        {
            FireResult result;
            // FIXME: This test is *very* costly, this function is the most frequently run in a POSH. 
            //        In lisp, I used to have a debug version of POSH that had lots of conditionals in it, 
            //        and a fast version with none.  Speaking of None, identity is faster to check than equality, 
            //        according to python.org
            //        Maybe profile.py should replace the function maned followDrive... note this would require 
            //        knowing if your posh was strict or scheduled.
            //        JJB 1 Mar 08
            if ( profiler is Profiler)
                profiler.increaseTotalCalls();

            log.Debug("Processing Drive Collection");

            result = dc.fire();
            timer.LoopEnd();

            if (result.continueExecution())
                return DRIVEFOLLOWED;
            else if (result.nextElement() is ElementCollection)
                return DRIVEWON;
            else 
                return DRIVELOST;
        }

        /// <summary>
        /// The loop thread, started by L{startLoop}.
        /// 
        /// This thread controls how L{followDrive} is called.
        /// </summary>
        public override void  loopThread()
        {
            int result;

            while (checkError(0) == 0)
            {
                // check for pause
                if (_loopPause)
                {
                    while (_loopPause)
                        // PARAMETER: the waiting time needs to be checked and parameterized
                        Thread.Sleep(10);
                    timer.Reset();
                    // check if stopLoop was called
                }
                if (!_execLoop)
                    return;
                // follow drive, and control the loop timing after that
                result = followDrive();
                if (result == DRIVEWON || result == DRIVELOST)
                    return;
                timer.LoopWait();
            }
        }

        /// <summary>
        /// Loads the plan and creates the drive collection tree.
        /// 
        /// The method parses the plan file, and then uses the plan builder to
        /// build the drive collection tree.
        /// </summary>
        /// <param name="planFile">Filename of the plan file that is loaded.</param>
        /// <returns></returns>
        public override void loadPlan(Stream plan)
        {
            // if setTimer() is not called, then the first use of
            // the timer will fail. setTimer() is called when the drive
            // collection is built.
            timer = null;
            // read plan, parse it and build drive collection
 	        PlanBuilder builder = new LAPParser().parse(new StreamReader(plan).ReadToEnd());
            dc = builder.build(this);
        }

    }
}