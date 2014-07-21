using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using POSH.sys;

namespace POSH.unity
{
    public abstract class POSHBehaviour : MonoBehaviour
    {
        protected POSHInnerBehaviour poshBehaviour;
        protected POSHController controller;
        protected AgentBase agent = null;

        /// <summary>
        /// Links the actual gameObject and its Component the POSHBehaviour to the POSHController.
        /// </summary>
        /// <param name="agent">The agent controlling the gameObject</param>
        /// <returns>A POSHBehaviour which is connected to the gameObject so that POSH is able to control Unity objects</returns>
        public sys.Behaviour LinkPOSHBehaviour(AgentBase agent)
        {
            if (this.agent == null || this.agent == agent)
            {
                this.agent = agent;
                Dictionary<string, object> parameters = agent.GetAttributes();
                poshBehaviour = InstantiateInnerBehaviour(agent);
            }
            else
            {
                GameObject clone = (GameObject) Instantiate(this.gameObject);
                poshBehaviour = clone.GetComponent<POSHBehaviour>().LinkPOSHBehaviour(agent) as POSHInnerBehaviour;
            }
            return poshBehaviour;
        }

        /// <summary>
        /// Needs tyo be implemented by an actual behaviour to allow the behaviour to specifically instantiate the POSHInnerBehaviour and set additional things in motion.
        /// Is called only from LinkPOSHBehaviour.
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        protected abstract POSHInnerBehaviour InstantiateInnerBehaviour(AgentBase agent);

        protected internal abstract void ConfigureParameters(Dictionary<string,object> parameters);

        protected internal abstract void ConfigureParameter(string parameter, object value);

        public void ConnectPOSHUnity(POSHController control)
        {
            controller = control;
        }

        public POSHInnerBehaviour GetInnerBehaviour()
        {
            return poshBehaviour;
        }




    }
}