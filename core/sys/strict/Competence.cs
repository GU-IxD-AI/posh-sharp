using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.events;

namespace POSH.sys.strict
{
    /// <summary>
    /// A Competence contains a list of CompetencePriorityElements that
    /// each contain some CompetenceElements. Upon firing a competence,
    /// the competence finds the first element in the competence priority list
    /// that executes successfully. A competence priority list executes
    /// successfully if at least one of its elements is ready to fire and is
    /// fired.
    /// </summary>
    public class Competence : ElementCollection
    {
        private List<CompetencePriorityElement> elements;
        private Trigger goal;

        /// <summary>
        /// A POSH competence, containing competence priority elements.
        /// 
        /// Initialises the competence.
        /// 
        /// If no goal is given, then the goal will never be reached.
        /// 
        /// The log domain is set to "[AgentId].C.[competence_name]".
        /// </summary>
        /// <param name="agent">The competence's agent.</param>
        /// <param name="competenceName">The name of the competence.</param>
        /// <param name="priorityElements">The priority elements of the competence,
        ///         in their order of priority.</param>
        /// <param name="goal">The goal of the competence.</param>
        public Competence(Agent agent, string competenceName, CompetencePriorityElement[] priorityElements, Trigger goal)
            :base(string.Format("C.{0}",competenceName),agent)
        {
            this.name = competenceName;
            if (priorityElements.Length > 0 )
                this.elements = new List<CompetencePriorityElement>(priorityElements);
            else
                this.elements = new List<CompetencePriorityElement>();
            this.goal = goal;

            log.Debug("Created");
        }

        /// <summary>
        /// Resets all the competence's priority elements.
        /// </summary>
        public override void  reset()
        {
            log.Debug("Reset");
            foreach (CompetencePriorityElement elem in elements)
                elem.reset();
 	        
        }

        /// <summary>
        /// Fires the competence.
        /// 
        /// This method first checks if the competence's goal is satisfied
        /// (if the goal is not None). If that is the case, then it
        /// returns FireResult(False, None). Otherwise it fires the
        /// priority elements one by one. On the first successful firing
        /// of a competence priority element, the method returns the
        /// result of the priority element. If no priority element fired
        /// successfully, then FireResult(False, None) is returned.
        /// </summary>
        /// <returns>The result of firing an element, or
        ///         FireResult(False, None)</returns>
        public override FireResult  fire()
        {
 	        log.Debug("Fired");
            FireArgs args = new FireArgs();
            
            // check if the goal is satisfied
            if (goal is Trigger && goal.fire())
            {
                log.Debug("Goal satisfied");
                args.FireResult = false;
                args.Time = DateTime.Now;
                BroadCastFireEvent(args);
                
                return new FireResult(false, null);
            }
            // process the elements
            FireResult result;
            foreach (CompetencePriorityElement elem in elements)
            {
                result = elem.fire();
                // check if the competence priority element failed
                if (result.continueExecution() && !(result.nextElement() is CopiableElement) )
                    continue;
                args.FireResult = result.continueExecution();
                args.Time = DateTime.Now;
                BroadCastFireEvent(args);
                
                return result;
            }
            // we failed
            log.Debug("Failed");
            args.FireResult = false;
            args.Time = DateTime.Now;
            BroadCastFireEvent(args);
            
            return new FireResult(false, null);
        }

        /// <summary>
        /// Returns a reset copy of itsself.
        /// 
        /// This method creates a copy of itsself that has a copy of the
        /// competence priority elements but is otherwise equal.
        /// </summary>
        /// <returns>A reset copy of itself.</returns>
        public override CopiableElement  copy()
        {
            // name and goal stays the same, only elements need to be copied
            // therefore we'll make a shallow copy of the object and
            // copy the elements separately
            Competence newObj = (Competence) this.MemberwiseClone();
            List<CompetencePriorityElement> newElements = new List<CompetencePriorityElement>();

            foreach (CompetencePriorityElement elem in elements)
                newElements.Add((CompetencePriorityElement) elem.copy());
            newObj.elements = newElements;

            return newObj;
        }

        /// <summary>
        /// Sets the list of priority elements of the competence.
        /// 
        /// Calling this method also resets the competence.
        /// </summary>
        /// <param name="elements"></param>
        public void setElements(CompetencePriorityElement[] elements)
        {
            this.elements = new List<CompetencePriorityElement>(elements);
            reset();
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            string plan = name;
            string c;
            elements = (elements is Dictionary<string, string>) ? elements : new Dictionary<string, string>();

            // taking appart the senses and putting them into the right form
            if (elements.ContainsKey(name))
                return plan;


            string acts = string.Empty;
            foreach (CompetencePriorityElement elem in this.elements)
            {
                acts += "\t(" + elem.ToSerialize(elements) + "\t)\n";
            }
            
            // TODO: the current implementation does not support timeouts
            c = String.Format("(C {0} {1} (goal {3})\n\t(elements \n{2} \n\t)\n)", name, "", acts,goal.ToSerialize(elements));
            elements[name] = c;
            return plan;
        }
    }
}
