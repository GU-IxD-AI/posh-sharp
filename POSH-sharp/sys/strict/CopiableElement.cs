using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys.strict
{
    /// <summary>
    /// An element that can be copied.
    /// 
    /// Any element that is or can become the element of a drive element
    /// or competence element as to be of this class or a inheriting class.
    /// </summary>
    class CopiableElement : ElementBase
    {
        /// <summary>
        /// Any element that is or can become the element of a drive element
        /// or competence element as to be of this class or a inheriting class.
        /// 
        /// Initialise the element.
        /// </summary>
        /// <param name="logDomain">The logging domain for the element.</param>
        /// <param name="agent">The agent that uses the element.</param>
        public CopiableElement(string logDomain, Agent agent)
            : base(logDomain, agent)
        {
        }

        /// <summary>
        /// Returns a reset copy of itself.
        /// 
        /// This method returns a copy of itself, by creating a new
        /// instance of itsself and replicating all state-dependent object
        /// variables. If the object variables are not state-dependent,
        /// they can be copied as references rather than real copies.
        /// 
        /// This method needs to be overriddent by inheriting classes.
        /// In its current implementation it raises NotImplementedError
        /// </summary>
        /// <returns>A copy of itsself.</returns>
        public virtual CopiableElement copy()
        {
            throw new NotImplementedException("CopiableElement.copy() needs to be overridden");
        }
    }
}