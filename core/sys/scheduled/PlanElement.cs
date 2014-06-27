using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys.scheduled
{
    /// <summary>
    /// An element of a POSH plan.
    /// </summary>
    public class PlanElement : CopiableElement
    {
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
    }
}