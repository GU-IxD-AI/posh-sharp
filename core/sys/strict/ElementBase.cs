using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.events;

namespace POSH.sys.strict
{
    /// <summary>
    /// A basic POSH element.
    ///
    /// A basic POSH element is any plan / behaviour element, like a drive,
    /// a drive element, an action pattern, a sense, ...
    ///
    /// Each such an element has a unique numeric id, that is
    /// assigned to the element upon creating it.
    ///
    /// This element is not used directly, but is inherited
    /// by L{POSH.strict.Sense}, L{POSH.strict.Action}, and
    /// L{POSH.strict.PlanElement}.
    /// </summary>
    public class ElementBase : LogBase
    {
        static int currentId = 0;
        protected int id;
        protected string name;

        ///
        /// Event Handling
        /// This creates FireEvents which can be used in the Fire method to allow for execution tracking
        /// an IListener needs to be used to subscribe to each plan element however
        ///
        public event FireHandler FireEvent;

        /// <summary>
        /// Returns a unique element id.
        /// This function returns an id for plan elements. At every call, 
        /// the internal id counter is increased by 1.
        /// </summary>
        /// <returns>A unique element id.</returns>
        static int getNextId()
        {
            return currentId += 1;
        }


        /// <summary>
        /// Initialises the element, and assigns it a unique id.
        /// </summary>
        /// <param name="logDomain">The logging domain for the element.</param>
        /// <param name="agent">The agent that uses the element.</param>
        public ElementBase(string logDomain, Agent agent)
            : base(logDomain, agent)
        {
            id = getNextId();
            name = "NoName";

        }

        /// <summary>
        /// Returns the name of the element.
        /// 
        /// The name has to be set by overriding classes by setting
        /// the object variable name
        /// </summary>
        /// <returns> The element's name.</returns>
        public string getName()
        {
            return name;
        }

        /// <summary>
        /// Returns the string representation of the element.
        /// </summary>
        /// <returns>[Classname] [Elementname] [Id]</returns>
        public override string ToString()
        {

            return string.Format("[{0} {1} {2}]", this.GetType().Name, name, id);
        }

        /// <summary>
        /// Returns the element's id.
        /// </summary>
        /// <returns>The element's id.</returns>
        public int getId()
        {
            return id;
        }

        /// <summary>
        /// TO convert a plan structure back intto its textual form we extract thei plan in POSH format.
        /// </summary>
        /// <param name="elements">The already existing elements so that duplicates can be treated more efficient.
        /// The dictionary will be altered if a new compentence or actionpattern is found.</param>
        /// <returns>THe posh plan representation of the element and its sub elements.
        /// The return value cis a string containing the tree structure opf the plan.</returns>
        public virtual string ToSerialize(Dictionary<string,string> elements)
        {
            throw new NotImplementedException(); 
        }
               
        protected void BroadCastFireEvent(EventArgs args)
        {
            // the event gernerates some weird issue when the listenener is not attached
            if (_agent_.HasListenerForTyp(EventType.Fire))
                FireEvent(EventType.Fire,this, args);
        }
    }
}
