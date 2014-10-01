using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys.strict
{
    /// <summary>
    /// The result of firing a plan element.
    /// 
    /// This result determines two things:
    ///     
    ///     - if we want to continue executing this part of the plan or want to
    ///       return to the root.
    ///     
    ///     - the plan element to execute in the next step, given that we are
    ///       continuing to execute the current plan of the step.
    /// 
    /// Continuing the execution means to either to fire the
    /// same element in the next execution step, or to descend further
    /// in the plan tree and fire the next element in that tree.
    /// The next element to execute also needs to be given. If this element
    /// is set to None, the element to execute stays the same. Otherwise
    /// the given element is copied, reset, and given as the next element.
    /// 
    /// If we are not continuing the execution of the current part of the
    /// plan, the currently fired drive element returns to the root of the plan.
    /// </summary>
    public class FireResult
    {
        private bool continueExecuting;
        private CopiableElement next;
        /// <summary>
        /// Initialises the result of firing an element.
        /// 
        /// For a more detailed description of the arguments, read the
        /// class documentation.
        /// </summary>
        /// <param name="continueExecution">If we want to continue executing the current
        /// part of the plan.</param>
        /// <param name="nextElement">The next plan element to fire.</param>
        public FireResult(bool continueExecution, CopiableElement nextElement)
        {
            continueExecuting = continueExecution;
            if (continueExecution && nextElement is CopiableElement)
                // copy the next element, if there is one
                // FIX: @swen: I do not see the need for copying loads of elements when they can be referenced instead.
                // FIXME: @ check if this still works when not cloned
                next = (ElementCollection)nextElement.copy();
            else
                next = null;
            // FIX: @swen: here must be an error in the original implementation I just uncommented the next line because it seemed wrong
            // next = nextElement;

        }

        /// <summary>
        /// Returns if we want to continue execution the current part of the
        /// plan.
        /// </summary>
        /// <returns>If we want to continue execution.</returns>
        public bool continueExecution()
        {
            return continueExecuting;
        }

        /// <summary>
        /// Returns the element to fire at the next step.
        /// </summary>
        /// <returns></returns>
        public CopiableElement nextElement()
        {
            return next;
        }

    }
}
