using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys;
using POSH_sharp.sys.annotations;
using Posh_sharp.examples.BODBot.util;

namespace Posh_sharp.examples.BODBot
{
    public class Andy : Behaviour
    {
        public Andy(AgentBase agent)
            : base(agent, new string[] {"StopBot",
                            "Rotate", "Walk",
                            "BigRotate", "MovePlayer", "PickupItem"},
                        new string[] {"SeeEnemyWithOurFlag",
                            "SeePlayer", "SeeItem",
                            "CloseToPlayer", "IsRotating",
                            "HitObject", "IsWalking", "IsStuck",
                            "Fail", "Succeed"})
        {
            
        }
        private BODBot getBot(string name="BODBot")
        {
            return ((BODBot)agent.getBehaviour(name));
        }

        /*
         * 
         * SENSES
         * 
         */
        [ExecutableSense("Fail")]
        public bool Fail()
        {
            return false;
        }

        [ExecutableSense("Succeed")]
        public bool Succeed()
        {
            return true;
        }

        [ExecutableSense("SeePlayer")]
        public bool SeePlayer()
        {
            if (getBot().viewPlayers.Count > 0)
                return true;

            return false;
        }

        [ExecutableSense("CloseToPlayer")]
        public bool CloseToPlayer()
        {
            // print "Checking to see if close to player..."
            // If we are really close to the first player
            int closeness = 50;
            
            if (getBot().viewPlayers.Count > 0)
                if (getBot().viewPlayers.First().Value.Location.Distance2DFrom(Vector3.ConvertToVector3(getBot().info["Location"]),Vector3.Orientation.XY) < closeness)
                    return true;
                
            return false;
        }

        [ExecutableSense("SeeItem")]
        public bool SeeItem()
        {
            // print "See an item?"
            
            if (getBot().viewItems.Count > 0)
                return true;
                
            return false;
        }

        [ExecutableSense("HitObject")]
        public bool HitObject()
        {
            // print "Was I hit?"
            if (getBot().WasHit() )
                // print "Yes!"
                return true;
                
            return false;
        }

        [ExecutableSense("IsRotating")]
        public bool IsRotating()
        {
            // print "IsRotating!"
            return getBot().Turning();
        }

        [ExecutableSense("IsWalking")]
        public bool IsWalking()
        {
            // print "IsWalking"
            
            return getBot().Moving();
        }

        [ExecutableSense("IsStuck")]
        public bool IsStuck()
        {
            // print "Stuck"
            
            return getBot().Stuck();
        }

        /*
         * 
         * ACTIONS
         * 
         */

        /// <summary>
        /// Finds the first player and move towards it
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("MovePlayer")]
        public bool MovePlayer()
        {
            // print "Move the player ..."
            if (getBot().viewPlayers.Count > 0)
            {
                getBot().SendMessage("RUNTO",new Dictionary<string,string> {{"Target",getBot().viewPlayers.First().Value.Id}});
                return true;
            }
            return false;
        }

        /// <summary>
        /// Pickup the first item on the list
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("PickupItem")]
        public bool PickupItem()
        {
            // print "Pickup item ..."
            if (getBot().viewItems.Count > 0)
            {
                getBot().SendMessage("RUNTO",new Dictionary<string,string> {{"Target",getBot().viewItems.First().Id}});
                return true;
            }
            return false;
        }

        /// <summary>
        /// Stops the Bot from doing stuff
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("StopBot")]
        public bool StopBot()
        {
            // print "Stopping Bot"
           getBot().SendMessage("STOP",new Dictionary<string,string>());
           
           return true;
        }

        /// <summary>
        /// Walking
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("Walk")]
        public bool Walk()
        {
            // print "Walking ..."
           
           return getBot().Move();;
        }

        /// <summary>
        /// Rotating the bot around itself (left or right)
        /// </summary>
        /// <param name="angle">If angle is not given random 90 degree turn is made</param>
        /// <returns></returns>
        [ExecutableAction("Rotating")]
        public bool Rotating(int angle = 0)
        {
            // print "Rotating ..."
            if (angle != 0)
                getBot().Turn(angle);
            else if (new Random().Next(2) == 0)
                // turn left
                getBot().Turn(90);
            else
                // turn right
                getBot().Turn(-90);


           return true;
        }

        /// <summary>
        /// Turns the bot 160 degrees
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("BigRotate")]
        public bool BigRotate()
        {
            // print "big rotate ..."
           
           return Rotating(160);
        }
    }
}






