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

        public sys.Behaviour LinkPOSHBehaviour(AgentBase agent)
        {
            if (this.agent == null || this.agent == agent)
            {
                this.agent = agent;
                poshBehaviour = InstantiateInnerBehaviour(agent);
            }
            else
            {
                GameObject clone = (GameObject) Instantiate(this.gameObject);
                poshBehaviour = clone.GetComponent<POSHBehaviour>().LinkPOSHBehaviour(agent) as POSHInnerBehaviour;
            }
            return poshBehaviour;
        }

        protected abstract POSHInnerBehaviour InstantiateInnerBehaviour(AgentBase agent);

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