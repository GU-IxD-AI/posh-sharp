using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace POSH.sys.scheduled
{

    /// <summary>
    /// A simple POSH plan element.
    /// 
    /// This element has besides the PlanElement an additional
    /// ready-state that is queried before it is fired.
    /// </summary>
    public class Element : PlanElement
    {
        /// <summary>
        /// A simple POSH plan element.
        /// 
        /// This element has besides the L{PlanElement} an additional
        /// ready-state that is queried before it is fired.
        /// 
        /// Initialises the element.
        /// </summary>
        /// <param name="logDomain">The logging domain for the element.</param>
        /// <param name="agent">The agent that uses the element.</param>
        public Element(string logDomain, Agent agent)
            : base(logDomain, agent)
        {
        }

        /// <summary>
        /// Returns if the element is ready to be fired.
        /// 
        /// This method needs to be overridden by inheriting classes.
        /// In its default implementation it raises NotImplementedError.
        /// </summary>
        /// <param name="timeStamp">The current timestamp in milliseconds.</param>
        /// <returns>If the element can be fired.</returns>
        public virtual bool isReady(long timeStamp)
        {
            throw new NotImplementedException("Element.isReady() needs to be overridden");
        }
    }
}
//}class Element(Base):
    
//    def __init__(self,
//                 trigger = None,
//                 drive_name = "non_assigned_element",
//                 **kw):
//        Base.__init__(self, **kw) # Call the ancestor init
//        self.trigger = trigger
//        self.drive_name = drive_name