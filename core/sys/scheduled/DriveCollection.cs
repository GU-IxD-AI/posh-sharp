﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys.scheduled
{
    /// <summary>
    /// Implementation of DriveCollection
    /// 
    /// A drive collection, containing drive priority elements.
    /// 
    /// A POSH.scheduled.DriveCollection contains several
    /// POSH.scheduled.DrivePriorityElement s
    /// that contains several POSH.scheduled.DriveElement s. Upon firing a drive
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
        public DriveCollection(Agent agent, string collectionName, DrivePriorityElement [] priorityElements, Trigger goal)
            : base(string.Format( "DC.{0}", collectionName),agent)
        {
            name = collectionName;
            elements = priorityElements;
            this.goal = goal;

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
        /// This method first checks if the goal (if not None) is met. If
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
    }
}