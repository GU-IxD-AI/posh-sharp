using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys.scheduled
{
 
    /// <summary>
    /// A drive element.
    /// </summary>
    public class DriveElement : Element
    {
        internal Trigger trigger;
        private Object root;
        private object element;
        private long maxFreq;
        private long lastFired;

        protected internal List<Behaviour> behaviours;
        
        Agent agent;
        public bool isLatched { get; internal protected set; }



        // TODO: replace root which should be a polymoph type (maybe create superclass)

        /// <summary>
        /// Initialises the drive element.
        /// 
        /// The log domain is set to [AgentName].DE.[element_name]
        /// </summary>
        /// <param name="agent">The element's agent.</param>
        /// <param name="elementName">The name of the drive element.</param>
        /// <param name="trigger">The trigger of the element.</param>
        /// <param name="root">The element's root element.
        ///     root is either POSH.scheduled.Action, POSH.scheduled.Competence or POSH.scheduled.ActionPattern
        ///     </param>
        /// <param name="maxFreq">The maximum frequency at which is element is
        ///     fired. The frequency is given in milliseconds between
        ///     invocation. A negative number disables this feature.</param>
        public DriveElement(Agent agent, string elementName, Trigger trigger, Object root, long maxFreq)
            : base(string.Format("DE.{0}", elementName), agent)
        {
            this.name = elementName;
            this.trigger = trigger;
            this.root = root;
            this.element = root;
            this.maxFreq = maxFreq;
            
            // the timestamp when it was last fired
            this.lastFired = -100000L;

            log.Debug("Created");
            this.agent = agent;
            this.isLatched = false;

            this.behaviours = new List<Behaviour>();

            foreach (POSHSense sense in trigger.senses)
                this.behaviours.Add(sense.behaviour);
        }

        /// <summary>
        /// Resets the drive element to its root element,
        /// and resets the firing frequency.
        /// </summary>
        public override void reset()
        {
            log.Debug("Reset");
            element = root;
            lastFired = -100000L;
        }

        /// <summary>
        /// Returns if the element is ready to be fired.
        ///
        /// The element is ready to be fired if its trigger is
        /// satisfied and if the time since the last firing is
        /// larger than the one given by C{maxFreq}. The time of the
        /// last firing is determined by the timestamp given
        /// to L{isReady} when it was called the last time and returned
        /// True. This implies that the element has to be fired
        /// every time when this method returns True.
        /// </summary>
        /// <param name="timeStamp">The current timestamp in milliseconds</param>
        /// <returns></returns>
        public override bool isReady(long timeStamp)
        {
            if (trigger.fire())
                if (maxFreq < 0 || timeStamp - lastFired > +maxFreq)
                {
                    lastFired = timeStamp;
                    return true;
                }
                else
                {
                    log.Debug("Max. firing frequency exceeded");
                }

            return false;
        }

        /// <summary>
        /// Fires the drive element.
        /// 
        /// This method fires the current drive element and always
        /// returns None. It uses the slip-stack architecture to determine
        /// the element to fire in the next step.
        /// </summary>
        /// <returns>The result returned is null.</returns>
        public override FireResult fire()
        {
            FireResult result;

            log.Debug("Fired");
            // if our element is an action, we just fire it and do
            // nothing afterwards. That's because we can only have an action
            // as an element, if it is the drive element's root element.
            // Hence, we didn't descend in the plan tree and can keep
            // the same element.
            
            if (element is POSHAction || element.GetType().IsSubclassOf(typeof(POSHAction)))
            {
                ((POSHAction)element).fire();
                element = root;
                return null;
            }

            // the element is a competence or an action pattern
            result = ((ElementCollection)element).fire();

            if (result.continueExecution())
            {
                // if we have a new next element, store it as the next
                // element to execute
                CopiableElement next = result.nextElement();
                if (next is CopiableElement)
                    element = next;
            }
            else 
                // we were told not to continue the execution -> back to root
                // We must not call reset() here, as that would also reset
                // the firing frequency of the element.
                element = root;

            return null;
        }

        public override CopiableElement copy()
        {
            throw new NotImplementedException("DriveElement.copy() is never supposed to be called");
        }
    }
}