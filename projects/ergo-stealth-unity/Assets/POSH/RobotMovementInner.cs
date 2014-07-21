using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using POSH.sys.annotations;
using UnityEngine;

namespace POSH.unity
{
    public class RobotMovementInner : POSHInnerBehaviour
    {
        public RobotMovementInner(AgentBase agent, POSHBehaviour parent, Dictionary<string, object> attributes)
            : base(agent, parent, attributes)
        {
            AttachToLog("init robot movement");
        }

        private RobotMovement GetRobotMovement()
        {
            return (RobotMovement)parent;
        }

        private void AttachToLog(string message)
        {
			log.Info("agent"+agent.id+": "+message);
        }

        // Actions
        [ExecutableAction("a_attack",0.01f)]
        public void a_attack()
        {
            GetRobotMovement().Shooting();
        }

        [ExecutableAction("a_charge", 0.01f)]
		public void a_charge()
        {
            GetRobotMovement().Recharging();
        }

        [ExecutableAction("a_retreat", 0.01f)]
		public void a_retreat() 
        {
            GetRobotMovement().Retreating();
        }

        [ExecutableAction("a_chase", 0.01f)]
        public void a_chase() 
        {
            GetRobotMovement().Chasing();
        }
        
        [ExecutableAction("a_patrol", 0.01f)]
        public void a_patrol() 
        {

            AttachToLog("in a_patrol");
            GetRobotMovement().Patrolling();
        }
        
        [ExecutableAction("a_idle", 0.01f)]
        public void a_idle() 
        {
            AttachToLog("in idle");
            GetRobotMovement().Idling(); 
       }

        /******************
         * Senses
         * ****************
         */

        [ExecutableSense("game_finished", 0.01f)]
        public bool game_finished()
        {
            return !GetRobotMovement().GameRunning();
        }

        [ExecutableSense("can_attack", 0.01f)]
        public bool can_attack()
        {
            //AttachToLog("in can_attack");
            return GetRobotMovement().alive;
        }

        [ExecutableSense("wants_to_attack", 0.01f)]
        public bool wants_to_attack()
        {
            AttachToLog("in wants_to_attack");
            //TODO: this is the place to integrate any kind of need model like ERGo
            return true;
        }

        [ExecutableSense("wants_to_chase", 0.01f)]
        public bool wants_to_chase()
        {
            //AttachToLog("in wants_to_chase");
            //TODO: this is the place to integrate any kind of need model like ERGo
            return GetRobotMovement().WantsToChase(); ;
        }


        [ExecutableSense("see_enemy", 0.01f)]
        public bool see_enemy()
        {
            AttachToLog("in see_enemy " + GetRobotMovement().playerInSight);
            return GetRobotMovement().playerInSight;
        }

        [ExecutableSense("saw_enemy", 0.01f)]
        public bool saw_enemy()
        {
            AttachToLog("in saw_enemy");
            if (GetRobotMovement().playerInSight == false && !GetRobotMovement().personalLastSighting.Equals(new Vector3(1000f, 1000f, 1000f)))
                return true;
            return false;
        }

        [ExecutableSense("need_to_charge", 0.01f)]
        public bool need_to_charge()
        {
            AttachToLog("in need_to_charge");
            return GetRobotMovement().WantsToCharge() && GetRobotMovement().alive;
         //   return false;
        }

        [ExecutableSense("wants_to_retreat", 0.01f)]
        public bool wants_to_retreat()
        {
            //AttachToLog("in wants_to_retreat");
            return false;
        }

        [ExecutableSense("wants_to_patrol", 0.01f)]
        public bool wants_to_patrol()
        {
            AttachToLog("in wants_to_patrol");
            return GetRobotMovement().WantsToPatrol() && GetRobotMovement().alive;
        }

        [ExecutableSense("success", 0.01f)]
        public bool success()
        {
            return true;
        }

        [ExecutableSense("fail", 0.01f)]
        public bool fail()
        {
            return false;
        }

    }
}
