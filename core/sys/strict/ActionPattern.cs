using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.events;

namespace POSH.sys.strict
{
    /// <summary>
    /// Implementation of an ActionPattern.
    /// </summary>
    public class ActionPattern : ElementCollection
    {
        List<CopiableElement> elements;
        private int elementIdx;

        /// <summary>
        /// Initialises the action pattern.
        /// 
        /// The log domain is set to [AgentId].AP.[patternName]
        /// </summary>
        /// <param name="agent">The corresponding agent.</param>
        /// <param name="patternName">The name of the action pattern.</param>
        /// <param name="elements">The sequence of actions or senses and 
        ///         an optional competence as the final element.</param>
        /// </param>
        public ActionPattern(Agent agent, string patternName, CopiableElement []elements)
            : base(string.Format("AP.{0}", patternName),agent)
        {
            name = patternName;
            this.elements = (elements.Length > 0) ? new List<CopiableElement>(elements) : new List<CopiableElement>();
            this.elementIdx = 0;
            log.Debug("Created");
        }

        /// <summary>
        /// Resets the action pattern.
        /// 
        /// This method sets the action pattern to fire the
        /// first action of the pattern upon the next call to L{fire}.
        /// </summary>
        public override void  reset()
        {
 	         log.Debug("Reset");
            this.elementIdx = 0;
        }

        /// <summary>
        /// Fires the action pattern.
        /// 
        /// This method fires the current action / sense / sense-act or
        /// competence of the pattern. In case of firing an action / sense
        /// / sense-act, the method points to the next element in the
        /// pattern and returns FireResult(True, None) if the current
        /// action / sense / sense-act was successful (i.e. evaluated to
        /// True) and not the last action in the sequence, in which case
        /// it returns FireResult(False, None) and resets the action
        /// pattern.
        /// 
        /// If the current element is a competence, then competence is
        /// returned as the next element by returning
        /// FireResult(True, competence), and the action pattern is
        /// reset.
        /// </summary>
        /// <returns>The result of firing the action pattern.</returns>
        public override FireResult  fire()
        {
 	        log.Debug("Fired");
            FireArgs args = new FireArgs();

            CopiableElement element = elements[elementIdx];
            if (element is POSHAction || element is POSHSense)
            {
                bool result;
                if (element is POSHAction)
                    result = ((POSHAction)element).fire().continueExecution();
                else
                    result = ((POSHSense)element).fire().continueExecution();

                if (!result)
                {
                    log.Debug(string.Format("Action/Sense {0} failed", element.getName()));
                    elementIdx = 0;
                    args.FireResult = result;
                    args.Time = DateTime.Now;

                    BroadCastFireEvent(args);
                    return new FireResult(false, null);
                }

                // check if we've just fired the last action
                elementIdx += 1;
                if (elementIdx >= elements.Count)
                {
                    elementIdx = 0;
                    args.FireResult = result;
                    args.Time = DateTime.Now;

                    BroadCastFireEvent(args);
                    return new FireResult(false, null);
                }
                args.FireResult = result;
                args.Time = DateTime.Now;

                BroadCastFireEvent(args);
                return new FireResult(true, null);
            }
            else if (element is Competence)
            {
                // we have a competence
                elementIdx = 0;
                args.FireResult = true;
                args.Time = DateTime.Now;

                BroadCastFireEvent(args);
                return new FireResult(true, element);
            }

            return null;
        }

        /// <summary>
        /// Returns a reset copy of itsself.
        ///
        /// This method returns a copy of itsself, and calls L{reset}
        /// on it.
        /// </summary>
        /// <returns>A reset copy of itsself.</returns>
        public override CopiableElement  copy()
        {
 	         ActionPattern newObj = (ActionPattern)this.MemberwiseClone();
            newObj.reset();
            
            return newObj;
        }

        /// <summary>
        /// Sets the elements of an action pattern.
        /// 
        /// Calling this method also resets the action pattern.
        /// </summary>
        /// <param name="elements">The list of elements of the action patterns. 
        ///         A sequence of Actions. An additional Competence can be the
        ///         last Element of the ActionPattern.</param>
        public void SetElements(CopiableElement [] elements)
        {
            this.elements = new List<CopiableElement>(elements);
            
            reset();
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            string plan = name;
            string ap;
            elements = (elements is Dictionary<string, string>) ? elements : new Dictionary<string, string>();

            // taking appart the senses and putting them into the right form
            if (elements.ContainsKey(name))
                return plan;


            string acts = string.Empty;
            foreach (CopiableElement elem in this.elements)
            {
                acts += "\t"+ elem.ToSerialize(elements) + "\n";
            }
            // TODO: the current implementation does not support timeouts
            ap = String.Format("(AP {0} {1} ( \n{2} \n))",name,"",acts);
            elements[name] = ap;
            return plan;
        }

    }
}