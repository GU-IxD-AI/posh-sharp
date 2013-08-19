using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys;
using POSH_sharp.sys.annotations;
using Posh_sharp.POSHBot.util;
using POSH_sharp.sys.strict;
//import utilityfns

namespace Posh_sharp.POSHBot
{
    public class Combat : UTBehaviour
    {
        internal CombatInfo info;

        public Combat(AgentBase agent)
            :base(agent,new string[] {"ShootEnemyCarryingOurFlag",
                            "RunToEnemyCarryingOurFlag",
                            "FaceAttacker", "SetAttacker", "ShootAttacker"},
                        new string[] {"SeeEnemyWithOurFlag",
                            "OurFlagOnGround", "EnemyFlagOnGround",
                            "IncomingProjectile",
                            "TakenDamageFromSpecificPlayer",
                            "TakenDamage", "IsRespondingToAttack"})
        {
            info = new CombatInfo();
        }
            
        /*
         * 
         * OTHER FUNCTIONS 
         * 
         */

        private void FindEnemyInView()
        {
            // work through who we can see, looking for an enemy
            string ourTeam = GetBot().info["Team"];
            Console.Out.WriteLine("Our Team: "+ourTeam);
            foreach(UTPlayer player in GetBot().viewPlayers.Values)
            {
                if (player.Team != ourTeam)
                {
                    // Turned KeepFocusOnID in to a tuple with the current_time as a timestamp FA
                    info.KeepFocusOnID = new Tuple<string,long>(player.Id,TimerBase.CurrentTimeStamp());
                    info.KeepFocusOnLocation = new Tuple<Vector3,long>(player.Location,TimerBase.CurrentTimeStamp());
                    return;
                }
            }
        }

        private void StopShooting()
        {
            if (info.GetFocusId() == null || info.GetFocusLocation() == null)
            {
                GetBot().SendMessage("STOPSHOOT", new Dictionary<string, string>());
            }
        }

       
        /// <summary>
        /// if its status is "held", update the CombatInfoClass to show who's holding it
        /// otherwise, set that to None as it means no-one is holding it
        /// </summary>
        /// <param name="values">Dictionary containing the Flag details</param>
        override internal void ReceiveFlagDetails(Dictionary<string,string> values)
        {
            // TODO: fix the mix of information in this method it should just contain relevant info
           // if (_debug_)
           //     Console.Out.WriteLine("in receiveFlagDetails");
            
            if ( GetBot().info == null ||  GetBot().info.Count < 1 )
                return;
            // set flag stuff
            if (values["Team"] == GetBot().info["Team"])
                if (values["State"].ToLower() == "held")
                    info.HoldingOurFlag = values["Holder"];
                else
                {
                    info.HoldingOurFlag = string.Empty;
                    info.HoldingOurFlagPlayerInfo = null;
                }
            else
            {
                if (values["State"].ToLower() == "held")
                {
                    if (GetBot().viewPlayers.ContainsKey(values["Holder"]))
                        info.HoldingEnemyFlagPlayerInfo = GetBot().viewPlayers["Holder"];
                    info.HoldingEnemyFlag = values["Holder"];
                }
                else
                {
                    info.HoldingEnemyFlag = string.Empty;
                    info.HoldingEnemyFlagPlayerInfo = null;
                }
            }
        }

        override internal void ReceiveProjectileDetails(Dictionary<string,string> values)
        {
            if (_debug_)
                Console.Out.WriteLine("received details of incoming projectile!");
            info.ProjectileDetails = new Projectile(values);
        }

        override internal void ReceiveDamageDetails(Dictionary<string,string> values)
        {
            if (_debug_)
                Console.Out.WriteLine("received details of damage!");
            info.DamageDetails = new Damage(values);
        }

        /// <summary>
        /// handle details about a player (not itself) dying
        /// remove any info about that player from CombatInfo
        /// </summary>
        /// <param name="values"></param>
        override internal void ReceiveKillDetails(Dictionary<string,string> values)
        {
            if (_debug_)
                Console.Out.WriteLine("received details of a kill!");
            
            info.ProjectileDetails = new Projectile(values);

            if (values["Id"] == info.HoldingOurFlag)
            {
                info.HoldingOurFlag = string.Empty;
                info.HoldingOurFlagPlayerInfo = null;
                GetBot().SendMessage("STOPSHOOT",new Dictionary<string,string>());
            }

            if (info.KeepFocusOnID != null && info.KeepFocusOnID.First != string.Empty)
                if (values["Id"] == info.KeepFocusOnID.First)
                {
                    info.GetFocusId();
                    info.GetFocusLocation();
                    GetBot().SendMessage("STOPSHOOT",new Dictionary<string,string>());
                }

        }

        override internal void ReceiveDeathDetails(Dictionary<string,string> value)
        {
            info.DamageDetails = null;
            info.KeepFocusOnID = null;
            info.KeepFocusOnLocation = null;
            GetBot().SendMessage("STOPSHOOT",new Dictionary<string,string>());
        }

        /*
         * 
         * SENSES
         * 
         */

        
        [ExecutableSense("SeeEnemyWithOurFlag")]
        public bool SeeEnemyWithOurFlag()
        {
            // print "in see_enemy_with_our_flag sense"
            if (GetBot().viewPlayers.Count == 0)
            {
                Console.Out.WriteLine("  no players visible");
                return false;
            }
            
            // check through every player we can see to check whether they're the one holding our flag
            foreach (string playerId in GetBot().viewPlayers.Keys)
            {
                if (playerId == info.HoldingOurFlag)
                {
                    Console.Out.WriteLine("  can see the player holding our flag");
                    info.HoldingOurFlagPlayerInfo = GetBot().viewPlayers[playerId];
                    return true;
                }
            }

            Console.Out.WriteLine(string.Format("  cannot see player '{0}' holding our flag.",info.HoldingOurFlag));
            return false;
        }

        [ExecutableSense("OurFlagOnGround")]
        public bool OurFlagOnGround()
        {
            // TODO: mixed parts of Movement again into different behaviour, needs to be cleaned later
            if ( GetMovement().info.HasOurFlagInfoExpired() )
                GetMovement().info.ExpireOurFlagInfo();

            if (GetMovement().info.ourFlagInfo.Count == 0)
                return false;
            else
            {
                // in case the flag was returned but we didn't actually see it happen
                if (!GetBot().gameinfo.ContainsKey("EnemyHasFlag"))
                    GetMovement().info.ourFlagInfo["State"] = "home";

                if (GetMovement().info.ourFlagInfo["State"].ToLower() == "dropped")
                {
                    Console.Out.WriteLine("our flag is dropped!");
                    return true;
                }
            }
            return false;
        }

        [ExecutableSense("EnemyFlagOnGround")]
        public bool EnemyFlagOnGround()
        {
            // TODO: remove interdependance on Movement
            if (GetMovement().info.HasEnemyFlagInfoExpired())
                GetMovement().info.ExpireEnemyFlagInfo();
            /*
             *  Made simpler FA
                By adding self.agent.Movement.PosInfo.EnemyFlagInfo["Reachable"] == "True" it has semi fixed the problem of the bot
                standing still after it has picked up the flag off the ground and dropped it off at base.
                This is because Reachable set to 0 on expiry of EnemyFlagInfo FA.
            */
            if (GetMovement().info.enemyFlagInfo.Count > 0 && GetMovement().info.enemyFlagInfo["State"].ToLower() == "dropped"
                && GetMovement().info.enemyFlagInfo["Reachable"] == "True")
                return true;

            return false;
        }

        [ExecutableSense("IncomingProjectile")]
        public bool IncomingProjectile()
        {
            if (info.GetProjectileDetails() is Projectile)
            {
                Console.Out.WriteLine("incoming-projectile returning 1");
                return true;
            }

            return false;
        }

        [ExecutableSense("TakenDamageFromSpecificPlayer")]
        public bool TakenDamageFromSpecificPlayer()
        {
            StopShooting();

            if (info.GetDamageDetails() is Damage && info.DamageDetails.AttackerID != string.Empty)
            {
                Console.Out.WriteLine(string.Format("Taken damage from specific player '{0}'; returning true ",info.DamageDetails.AttackerID));
                return true;
            }
            else if (info.KeepFocusOnLocation is Tuple<Vector3,long>)
                return true;

            return false;
        }
        /// <summary>
        /// expire damage info if necassary FA
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("TakenDamage")]
        public bool TakenDamage()
        {
            if (info.GetDamageDetails() is Damage)
                return true;
            
            return false;
        }

        /*
         * 
         * ACTIONS 
         * 
         */

        [ExecutableAction("ShootEnemyCarryingOurFlag")]
        public bool ShootEnemyCarryingOurFlag()
        {
            Console.Out.WriteLine(" in EnemyCarryingOurFlag");
            if (info.HoldingOurFlag != string.Empty && info.HoldingOurFlagPlayerInfo is UTPlayer)
            {
                GetBot().SendMessage("SHOOT", new Dictionary<string,string>()
                    {
                        {"Target", info.HoldingOurFlag},
                        {"Location", info.HoldingOurFlagPlayerInfo.Location.ToString()}
                    });
                return true;
            }
            return false;
        }

        [ExecutableAction("RunToEnemyCarryingOurFlag")]
        public bool RunToEnemyCarryingOurFlag()
        {
            Console.Out.WriteLine(" in ShootEnemyCarryingOurFlag");
            if (info.HoldingOurFlag != string.Empty && info.HoldingOurFlagPlayerInfo is UTPlayer)
            {
                Console.Out.WriteLine("in ShootEnemyCarryingOurFlag: a Player is holding our Flag");
                GetBot().SendIfNotPreviousMessage("RUNTO", new Dictionary<string,string>()
                    {
                        {"Location",info.HoldingOurFlagPlayerInfo.Location.ToString()},
                    });
                return true;
            }
            return false;
        }

        /// <summary>
        /// we can see the player currently, store his ID so e.g. runtos will be replaced 
        /// by strafes to keep him in focus and issue a turnto command
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("FaceAttacker")]
        public bool FaceAttacker()
        {
            StopShooting();

            if (info.GetFocusId() == null && info.GetFocusLocation() == null)
                return false;
            if (info.KeepFocusOnID == null)
                GetBot().SendMessage("TURNTO", new Dictionary<string,string>()
                    {
                    {"Location",info.KeepFocusOnLocation.First.ToString()}
                    });
            else 
                GetBot().SendMessage("TURNTO", new Dictionary<string,string>()
                    {
                    {"Target",info.KeepFocusOnID.First.ToString()}
                    });
            return true;
        }

        /// <summary>
        /// sets the attacker (i.e. the keepfocuson one) to be the first enemy player we have seen
        /// or the instigator of the most recent damage, if we know who that is
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("SetAttacker")]
        public bool SetAttacker()
        {
            Console.Out.WriteLine(" in SetAttacker");

            if (GetBot().viewPlayers.Count == 0 || GetBot().info.Count == 0)
                return false;

            if (info.GetDamageDetails() is Damage && info.DamageDetails.AttackerID != "")
                if ( GetBot().viewPlayers.ContainsKey(info.DamageDetails.AttackerID) )
                {
                    // set variables so that other commands will keep him in view
                    // Turned KeepFocusOnID into a tuple with the current_time as a timestamp FA
                    info.KeepFocusOnID = new Tuple<string,long>(info.DamageDetails.AttackerID,TimerBase.CurrentTimeStamp());
                    info.KeepFocusOnLocation = new Tuple<Vector3,long>(GetBot().viewPlayers[info.DamageDetails.AttackerID].Location,TimerBase.CurrentTimeStamp());
                }
                else
                    FindEnemyInView();
            else
                FindEnemyInView();
            
            return true;
        }

        [ExecutableAction("ShootAttacker")]
        public bool ShootAttacker()
        {
            Console.Out.WriteLine(" in ShootAttacker");

            StopShooting();

            if (info.GetFocusLocation() == null)
                return false;
                        
            if (info.GetFocusId() == null)
                GetBot().SendIfNotPreviousMessage("SHOOT",new Dictionary<string,string>()
                    {
                        {"Location",info.KeepFocusOnLocation.First.ToString()}
                    });
            else
                GetBot().SendIfNotPreviousMessage("SHOOT",new Dictionary<string,string>()
                    {
                        {"Target",info.KeepFocusOnID.First},
                        {"Location",info.KeepFocusOnLocation.First.ToString()}
                    });
            return true;
        }

    }
}     

    
