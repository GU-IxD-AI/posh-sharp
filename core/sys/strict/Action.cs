using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.events;

namespace POSH.sys.strict
{
    /// <summary>
    /// Implementation of an Action.
    /// 
    /// An action as a thin wrapper around a behaviour's action method.
    /// </summary>
    public class POSHAction : PlanElement
    {
        BehaviourDict behaviourDict;
        private Tuple<string,Behaviour> action;
        Behaviour behaviour;
       

        /// <summary>
        /// Picks the given action from the given agent.
        /// 
        /// The method uses the agent's behaviour dictionary to get the
        /// action method.
        /// 
        /// The log domain is set to "[AgentId].Action.[action_name]".
        /// 
        /// The action name is set to "[BehaviourName].[action_name]".
        /// </summary>
        /// <param name="agent">The agent that can perform the action.</param>
        /// <param name="actionName">The name of the action</param>
        public POSHAction(Agent agent, string actionName)
            :base(string.Format("Action.{0}",actionName),agent)
        {
            behaviourDict = agent.getBehaviourDict();
            action = behaviourDict.getAction(actionName);
            behaviour = behaviourDict.getActionBehaviour(actionName);
            name = string.Format("{0}.{1}",behaviour.GetName(),actionName);
            log.Debug("Created");

            //hook up to a listener for fire events
            if (agent.HasListenerForTyp(EventType.Fire))
                agent.SubscribeToListeners(this, EventType.Fire);
        }

        /// <summary>
        /// Performs the action and returns if it was successful.
        /// </summary>
        /// <returns>True if the action was successful, and False otherwise.</returns>
        public override FireResult fire()
        {
            bool success = action.Second.ExecuteAction(action.First);
            FireArgs args = new FireArgs();
            args.FireResult = success;
            args.Time = DateTime.Now;
            
            BroadCastFireEvent(args);

            log.Debug("Firing");
            return new FireResult(success,null);
        }

        /// <summary>
        /// Returns itsself.
        /// 
        /// This method does NOT return a copy of the action as the action
        /// does not have an internal state and therefore doesn't need to
        /// be copied.
        /// </summary>
        /// <returns></returns>
        public override CopiableElement copy()
        {
            return this;
        }

        public override string ToSerialize(Dictionary<string, string> elements)
        {
            return name.Split('.').Last();
        }
        
    }
}