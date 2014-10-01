using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.events;

namespace POSH.sys.strict
{
    /// <summary>
    /// An element of a POSH plan.
    /// </summary>
    public class PlanElement : CopiableElement
    {

        ///
        /// Event Handling
        /// This creates FireEvents which can be used in the Fire method to allow for execution tracking
        /// an IListener needs to be used to subscribe to each plan element however
        ///
        public event FireHandler FireEvent;
        

        /// <summary>
        /// An element of a POSH plan.
        /// </summary>
        /// <param name="logDomain">The logging domain for the element.</param>
        /// <param name="agent">The agent that uses the element.</param>
        public PlanElement(string logDomain, Agent agent) 
            : base(logDomain,agent)
        {
        }

        /// <summary>
        /// Resets the element.
        /// 
        /// This method has to be overridden in inheriting classes.
        /// In its default implementation is raises NotImplementedError.
        /// </summary>
        public virtual void reset()
        {
            throw new NotImplementedException("PlanElement.reset() needs to be overridden");
        }

        /// <summary>
        /// Fires the element and returns the result.
        /// 
        /// The result is given as a FireResult object.
        /// This method needs to be overriden by inheriting classes.
        /// In its default implementation is raises NotImplementedError.
        /// </summary>
        /// <returns>The result of firing the element.</returns>
        public virtual FireResult fire()
        {
            throw new NotImplementedException("PlanElement.fire() needs to be overridden");
        }

        protected void BroadCastFireEvent(EventArgs args)
        {
            // the event gernerates some weird issue when the listenener is not attached
            if (_agent_.HasListenerForTyp(EventType.Fire))
                FireEvent(EventType.Fire,this, args);
        }
    }
}