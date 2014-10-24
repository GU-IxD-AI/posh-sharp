using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.events;

namespace POSH.sys.strict
{
    /// <summary>
    /// A competence priority element, containing competence elements.
    /// </summary>
    public class CompetencePriorityElement : ElementCollection
    {
        private List<CompetenceElement> elements;

        /// <summary>
        /// Initialises the competence priority element.
        /// 
        /// The log domain is set to [AgentName].CP.[competence_name]
        /// </summary>
        /// <param name="agent">The element's agent.</param>
        /// <param name="CompetenceName">The name of the competence.</param>
        /// <param name="elements">The set of competence elements of the
        ///         priority element.</param>
        public CompetencePriorityElement(Agent agent, string competenceName, CompetenceElement[] elements)
            : base(string.Format("CP.{0}",competenceName),agent)
        {
            this.name = competenceName;
            if (elements.Length > 0)
                this.elements = new List<CompetenceElement>(elements);
            else
                this.elements = new List<CompetenceElement>();

            log.Debug("Created");
        }

        /// <summary>
        /// Resets all its competence elements.
        /// </summary>
        public override void  reset()
        {
 	        log.Debug("Reset");
            foreach (CompetenceElement elem in elements)
                elem.reset();
        }

        /// <summary>
        /// Fires the competence priority element.
        /// 
        /// This method goes through its list of competence elements
        /// and fires the first one that is ready. In that case,
        /// the result of the competence element is returned. Otherwise,
        /// it returns FireResult(True, None) (this can never be returned
        /// by a competence element and is therefore uniquely identifyable).
        /// </summary>
        /// <returns>The result of firing the competence priority element.</returns>
        public override FireResult  fire()
        {
 	        log.Debug("Fired");
            FireArgs args = new FireArgs();

            foreach (CompetenceElement elem in elements)
            {
                // as the method ignores the timestamp, we can give it
                // whatever we want
                if (elem.isReady(0))
                {
                    FireResult result =  elem.fire();
                    args.FireResult = result.continueExecution();
                    args.Time = DateTime.Now;
                    BroadCastFireEvent(args);

                    return result;
                }
            }
            log.Debug("Priority Element failed");
            
            args.FireResult = true;
            args.Time = DateTime.Now;
            BroadCastFireEvent(args);

            return new FireResult(true, null);
        }

        /// <summary>
        /// Returns a reset copy of itsself.
        /// 
        /// This method creates a copy of itsself that has a copy of the
        /// reset priority elements but is otherwise equal.
        /// </summary>
        /// <returns></returns>
        public override CopiableElement  copy()
        {
            // everything besides the elements stays the same. That's why 
            // we make a shallow copy and only copy the elements separately.

            CompetencePriorityElement newObj = (CompetencePriorityElement) this.MemberwiseClone();
            List<CompetenceElement> newElements = new List<CompetenceElement>();
 	        
            foreach (CompetenceElement elem in elements )
                newElements.Add((CompetenceElement) elem.copy());
            newObj.elements = newElements;

            return newObj;
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            string plan = string.Empty;
            elements = (elements is Dictionary<string, string>) ? elements : new Dictionary<string, string>();

            // taking appart the senses and putting them into the right form
            

            foreach (CompetenceElement elem in this.elements)
            {
                plan += "\t(" + elem.ToSerialize(elements) + ")";
            }

            return plan;
        }
    }
}
