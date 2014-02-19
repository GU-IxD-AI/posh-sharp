using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys.strict
{
    /// <summary>
    /// A competence element.
    /// </summary>
    public class CompetenceElement : Element
    {
        private Trigger trigger;
        private CopiableElement element;
        private int maxRetries;
        private int retries;
        /// <summary>
        /// Initialises the competence element.
        /// 
        /// The log domain is set to [AgentName].CE.[element_name].
        /// </summary>
        /// <param name="agent">The competence element's agent.</param>
        /// <param name="elementName">The name of the competence element.</param>
        /// <param name="trigger">The element's trigger</param>
        /// <param name="element">The element to fire (Action,Competence or ActionPattern).</param>
        /// <param name="maxRetries">The maximum number of retires. If this is set
        ///         to a negative number, it is ignored.</param>
        public CompetenceElement(Agent agent, string elementName, Trigger trigger, CopiableElement element, int maxRetries)
            :base(string.Format("CE.{0}",elementName),agent)
        {
            this.name = elementName;
            this.trigger = trigger;
            this.element = element;
            this.maxRetries = maxRetries;
            this.retries = 0;

            log.Debug("Created");
        }

        /// <summary>
        /// Resets the retry count.
        /// </summary>
        public override void  reset()
        {
 	         retries = 0;
        }

        /// <summary>
        /// Returns if the element is ready to be fired.
       /// 
       /// The element is ready to be fired if its trigger is
       /// satisfied and it was not fired more than maxRetries.
       /// Note that timestamp is ignored in this method. It is only
       /// there because isReady is defined like that in the
       /// POSH.strict.Element interface.
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns>If the element is ready to be fired.</returns>
        public override bool  isReady(long timeStamp)
        {
 	         if (trigger.fire())
                 if (maxRetries < 0 || retries < maxRetries )
                 {
                     retries +=1;
                     return true;
                 } else
                    log.Debug("Retry limit exceeded");
            return false;
        }

        /// <summary>
        /// Fires the competence element.
        /// 
        /// If the competence element's element is an Action, then this
        /// action is executed and FireResult(False, None) is returned.
        /// Otherwise, FireResult(True, element) is returned,
        /// indicating that at the next execution step that element has
        /// to be fired.
        /// </summary>
        /// <returns>Result of firing the competence element.</returns>
        public override FireResult  fire()
        {
 	        log.Debug("Fired");
            if (element is POSHAction){
                ((POSHAction)element).fire();
                return new FireResult(false, null);
            }
            return new FireResult(true, element);
        }

        /// <summary>
        /// Returns a reset copy of itsself.
        /// 
        /// This method creates a copy of itsself that links to the
        /// same element, but has a reset retry counter.
        /// </summary>
        /// <returns>A reset copy of itself.</returns>
        public override CopiableElement  copy()
        {
            CompetenceElement newObj = (CompetenceElement) this.MemberwiseClone();
            newObj.reset();
            
            return newObj;
        }
    }
}
//class Competence_Element(Element):
    
//    def __init__(self,
//                 ce_label = "fix_me_cel",
//                 action = None,
//                 retries = -1,
//                 competence = "not_my_competence",
//                 **kw):
//        # Call ancestor init with the remaining keywords
//        Element.__init__(self, **kw)
//        self.ce_label = ce_label
//        self.action = action
//        self.retries = retries
//        self.competence = competence

//    # Determine if the element is ready by first cheacking the
//    # preconditions the action object
//    def ready(self):
//        for this_precon in self.action.preconditions:
//            if not self.agent.blackboard.find_prereq(command = \
//                                                     this_precon.command,
//                                                     tag = this_precon.tag):
              
//                return 0
//        if self.trigger:
	    

//            return self.trigger.trigger()

//        else:
//            return 1