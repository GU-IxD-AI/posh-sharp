using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys.parse;

namespace POSH_sharp.sys.strict
{
    /// <summary>
    /// A drive priority element, containing drive elements.
    /// </summary>
    public class DrivePriorityElement : ElementCollection
    {
        private List<DriveElement> elements;
        TimerBase timer;
        private Agent agent;
        private List<Behaviour> behaviours; 


        /// <summary>
        /// Initialises the drive priority element.
        /// 
        /// The log domain is set to [AgentName].DP.[drive_name]
        /// </summary>
        /// <param name="agent">The element's agent.</param>
        /// <param name="driveName">The name of the associated drive.</param>
        /// <param name="elements">The drive elements of the priority element.</param>
        public DrivePriorityElement(Agent agent, string driveName, DriveElement [] elements)
            : base(string.Format("DP.{0}", driveName), agent)
        {
            name = driveName;
            this.elements = new List<DriveElement>(elements);
            timer = agent.getTimer();
            this.agent = agent;

            log.Debug("Created");
        }

        /// <summary>
        /// Resets all drive elements in the priority element.
        /// </summary>
        public override void reset()
        {
            log.Debug("Reset");
            foreach (DriveElement elem in elements)
                elem.reset();
        }

        /// <summary>
        /// Fires the drive prority element.
        /// 
        /// This method fires the first ready drive element in its
        /// list and returns FireResult(False, None). If no
        /// drive element was ready, then None is returned.
        /// </summary>
        /// <returns>The result of firing the element.</returns>
        public override FireResult fire()
        {
            log.Debug("Fired");
            long timeStamp = timer.time();
            elements = LAPParser.ShuffleList(elements);
            // new_elements=self.get_sorted_drive()

            if (elements.Contains(agent.dc.lastTriggeredElement))
                if (agent.dc.lastTriggeredElement.isReady(timeStamp))
                {
                    //if not self.agent._dc.last_triggered_element._behaviours[0].wants_to_interrupt():
                    //    for element in new_elements:
                    //        if element.isReady(timestamp) and element._behaviours[0].wants_to_interrupt():#and element!=self.agent._dc.last_triggered_element
                    //            self.agent._dc.last_triggered_element=element
                    //            element.fire()
                    //            return FireResult(False, None)
                    agent.dc.lastTriggeredElement.fire();
                    return new FireResult(false, null);
                }
            // for element in new_elements:
            foreach (DriveElement element in elements)
            {
                if (element.isReady(timeStamp))
                {
                    if (element != agent.dc.lastTriggeredElement)
                        if (agent.dc.lastTriggeredElement == null)
                        {
                            if (element.isLatched)
                                agent.dc.lastTriggeredElement = element;
                        }
                        else if (!agent.dc.lastTriggeredElement.isReady(timeStamp))
                            // event finished natually
                            agent.dc.lastTriggeredElement = null;
                        else
                        {
                            behaviours = agent.dc.lastTriggeredElement.behaviours;

                            // TODO: check this latching behaviour thing
                            foreach (Behaviour b in behaviours)
                                if ( b.GetType().IsSubclassOf(typeof(LatchedBehaviour)))
                                   ((LatchedBehaviour)b).signalInterrupt();

                            if (element.isLatched)
                                agent.dc.lastTriggeredElement = element;
                            else
                                agent.dc.lastTriggeredElement = null;
                        }
                    element.fire();
                    return new FireResult(false, null);
                }
            }

            return null;
        }

        public List<DriveElement> getSortedDrive()
        {
            // List<DriveElement> allElements;

            // TODO: if this is really needed it must be implemented
            throw new NotImplementedException();

            //    def get_sorted_drive(self):
            //        all_elements=[]
            //        for index,element in enumerate(self._elements):
            //            if element.is_latched:
            //                all_elements.append((element._behaviours[0].get_urgency(),index))
            //            else:
            //                all_elements.append((0,index))
            //        all_elements.sort()
            //        new_elements=[]
            //        for pair in all_elements:
            //            new_elements.append(self._elements[pair[1]])
            //        return new_elements  
        }

        /// <summary>
        /// Is never supposed to be called and raises an error.
        /// </summary>
        /// <returns>DrivePriorityElement.copy() is never supposed to be called</returns>
        public override CopiableElement copy()
        {
            throw new NotImplementedException("DrivePriorityElement.copy() is never supposed to be called");
        }
    }
}