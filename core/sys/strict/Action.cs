using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys.strict
{
    /// <summary>
    /// Implementation of an Action.
    /// 
    /// An action as a thin wrapper around a behaviour's action method.
    /// </summary>
    public class Action : CopiableElement
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
        public Action(Agent agent, string actionName)
            :base(string.Format("Action.{0}",actionName),agent)
        {
            behaviourDict = agent.getBehaviourDict();
            action = behaviourDict.getAction(actionName);
            behaviour = behaviourDict.getActionBehaviour(actionName);
            name = string.Format("{0}.{1}",behaviour.GetName(),actionName);
            log.Debug("Created");
        }

        /// <summary>
        /// Performs the action and returns if it was successful.
        /// </summary>
        /// <returns>True if the action was successful, and False otherwise.</returns>
        public bool fire()
        {
            log.Debug("Firing");
            return (action.Second.ExecuteAction(action.First));
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
        
    }
}