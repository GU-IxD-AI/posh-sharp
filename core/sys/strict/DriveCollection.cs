using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys.strict
{
    /// <summary>
    /// Implementation of DriveCollection
    /// 
    /// A drive collection, containing drive priority elements.
    /// 
    /// A POSH.strict.DriveCollection contains several
    /// POSH.strict.DrivePriorityElement s
    /// that contains several POSH.strict.DriveElement s. Upon firing a drive
    /// collection, either the goal is satisfied, or either of the drive
    /// priority elements needs to be fired successfully. Otherwise, the
    /// drive fails. The drive priority elements are tested in order or
    /// their priority. A drive priority element fires successfully if one
    /// of its drive elements is ready and can be fired.
    /// </summary>
    public class DriveCollection : ElementCollection
    {
        protected internal DrivePriorityElement[] elements;
        protected internal Trigger goal;
        protected internal DriveElement lastTriggeredElement;

        private string type;

        /// <summary>
        /// Initialises the drive collection.
        /// 
        /// The log domain is set to [AgentId].DC.[collection_name]
        /// 
        /// If no goal is given (goal = None), then it can never be satisfied.
        /// </summary>
        /// <param name="agent">The collection's agent.</param>
        /// <param name="collectionName">The name of the drive collection.</param>
        /// <param name="priorityElements">The drive elements in order of their
        ///         priority, starting with the highest priority.</param>
        /// <param name="goal">The goal of the drive collection.</param>
        public DriveCollection(Agent agent, string collectionType, string collectionName, DrivePriorityElement[] priorityElements, Trigger goal)
            : base(string.Format( "SDC.{0}", collectionName),agent)
        {
            name = collectionName;
            elements = priorityElements;
            this.goal = goal;
            type = collectionType;

            log.Debug("Created");
        }

        /// <summary>
        /// Resets all the priority elements of the drive collection.
        /// </summary>
        public override void  reset()
        {
 	         log.Debug("Reset");
            foreach(DrivePriorityElement elem in elements)
                elem.reset();
        }

        /// <summary>
        /// Fires the drive collection.
        /// 
        /// This method first checks if the goal (if not null) is met. If
        /// that is the case, then FireResult(False, self) is
        /// returned. Otherwise it goes through the list of priority
        /// elements until the first one was fired successfully (returning
        /// something else than None). In that case, FireResult(True,
        /// None) is returned. If none of the priority elements were
        /// successful, FireResult(False, None) is returned, indicating a
        /// failing of the drive collection.
        /// 
        /// To summarise:
        ///     - FireResult(True, None): drive element fired
        ///     - FireResult(False, self): goal reached
        ///     - FireResult(False, None): drive failed
        /// </summary>
        /// <returns>The result of firing the drive.</returns>
        public override FireResult  fire()
        {
            log.Debug("Fired");

            // check if goal reached
            if (goal is Trigger && goal.fire())
            {
                log.Debug("Goal Satisfied");
                return new FireResult(false, this);
            }

            // fire elements
            foreach (DrivePriorityElement elem in elements)
                // a priority element returns None if it wasn't
                // successfully fired
                if (elem.fire() != null)
                    return new FireResult(true, null);

            // drive failed (no element fired)
            log.Debug("Failed");

            return new FireResult(false, null);
        }

        /// <summary>
        /// Is never supposed to be called and raises an error.
        /// </summary>
        /// <returns>DriveCollection.copy() is never supposed to be called</returns>
        public override CopiableElement  copy()
        {
 	         throw new NotImplementedException("DriveCollection.copy() is never supposed to be called");
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            string dc;
            elements = (elements is Dictionary<string, string>) ? elements : new Dictionary<string, string>();

            
            string acts = string.Empty;
            foreach (DrivePriorityElement elem in this.elements)
            {
                acts += "\t(" + elem.ToSerialize(elements) + "\t)\n";
            }

            // TODO: the current implementation does not support timeouts
            dc = String.Format("({0} {1} (goal {3})\n\t(drives \n{2} \n\t)\n)", type, name, acts, goal.ToSerialize(elements));
            
            return dc;
        }
    }
}