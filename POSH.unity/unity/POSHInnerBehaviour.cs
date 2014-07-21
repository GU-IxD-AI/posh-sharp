using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using POSH.sys.annotations;

namespace POSH.unity
{
    public abstract class POSHInnerBehaviour : Behaviour
    {
        protected POSHBehaviour parent;

        /// <summary>
        /// Initialises behaviour with given actions and senses.
        /// 
        /// The actions and senses has to correspond to
        ///   - the method names that implement those actions/senses
        ///   - the names used in the plan
        /// 
        /// The log domain of a behaviour is set to
        /// [AgentId].Behaviour
        /// </summary>
        /// <param name="agent">The agent that uses the behaviour</param>
        /// <param name="actions">The action names to register.</param>
        /// <param name="senses">The sense names to register.</param>
        /// <param name="attributes">List of attributes to initialise behaviour state.</param>
        /// <param name="caller"></param>
        public POSHInnerBehaviour(AgentBase agent, POSHBehaviour parent, Dictionary<string, object> attributes)
            : base(agent, null, null, attributes, null)
        {
            this.parent = parent;
        }

        public override void AssignAttributes(Dictionary<string, object> attribs)
        {
            base.AssignAttributes(attribs);
            parent.ConfigureParameters(attribs);
        }

        public override void AssignAttribute(string key, object attrib)
        {
            base.AssignAttribute(key, attrib);
            parent.ConfigureParameter(key, attrib);
        }
        

    }
}
