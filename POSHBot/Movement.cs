using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys;
using POSH_sharp.sys.annotations;
using Posh_sharp.POSHBot.util;
using POSH_sharp.sys.strict;

namespace Posh_sharp.POSHBot
{
    public class Movement : Behaviour
    {
        internal PositionsInfo posInfo;
        string pathHomeId;
        string reachPathHomeId;
        string pathToEnemyBaseId;
        string reachPathToEnemyBaseID;

        public Movement(AgentBase agent) 
            : base(agent, 
            new string[] {"WalkToNavPoint", "ToEnemyFlag", 
                            "ToOwnBase", "ToOwnFlag", "ToEnemyBase", 
                            "RuntoMedicalKit", "RuntoWeapon"},
            new string[] {"AtEnemyBase", "AtOwnBase", "KnowEnemyBasePos",
                            "KnowOwnBasePos", "ReachableNavPoint",
                            "EnemyFlagReachable", "OurFlagReachable",
                            "SeeEnemy", "SeeReachableMedicalKit",
                            "SeeReachableWeapon", "TooCloseForPath"})
        {
            this.posInfo = new PositionsInfo();
            pathHomeId = "PathHome";
            reachPathHomeId = "ReachPathHome";
            pathToEnemyBaseId = "PathThere";
            reachPathToEnemyBaseID = "ReachPathThere";

        }


        /*
         * 
         * internal methods
         * 
         */ 

        /// <summary>
        /// updates the flag positions in PositionsInfo
        /// also updates details of bases, if relevant info sent
        /// the position of a flag is how we determine where the bases are
        /// </summary>
        /// <param name="values">Dictionary containing the Flag details</param>
        internal void ReceiveFlagDetails(Dictionary<string,string> values)
        {
            // TODO: fix the mix of information in this method it should just contain relevant info

            Console.Out.WriteLine("in receiveFlagDetails");
            Console.Out.WriteLine(values.ToArray().ToString());

            if ( getBot().info == null ||  getBot().info.Count < 1 )
                return;
            // set flag stuff
            if ( values["Team"] == getBot().info["Team"] )
                posInfo.ourFlagInfo = values;
            else
                posInfo.enemyFlagInfo = values;

            if (values["State"] == "home")
                if ( values["Team"] == getBot().info["Team"] )
                    posInfo.ownBasePos = NavPoint.ConvertToNavPoint(values);
                else
                    posInfo.enemyBasePos = NavPoint.ConvertToNavPoint(values);
        }

        /// <summary>
        /// if the 'ID' key is PathHome then it tells the bot how to get home.
        /// we need to turn the dictionary into a list, ordered by key ('0' ... 'n')
        /// at present other IDs are ignored
        /// </summary>
        /// <param name="valuesDict"></param>
        internal void ReceivePathDetails(Dictionary<string,string> valuesDict)
        {
            if (!valuesDict.ContainsKey("ID") )
                return;
            if (valuesDict["ID"] == pathHomeId )
                posInfo.pathHome = NavPoint.ConvertToPath(valuesDict);
            else if (valuesDict["ID"] == pathToEnemyBaseId )
                posInfo.pathToEnemyBase = NavPoint.ConvertToPath(valuesDict);

            // if there's no 0 key we're being given an empty path, so set TooCloseForPath to the current time
            // so that we can check how old the information is later on
            if ( !valuesDict.ContainsKey("0") )
                posInfo.TooCloseForPath = TimerBase.CurrentTimeStamp();
            else
                posInfo.TooCloseForPath = 0L;
        }

        /// <summary>
        /// used in validating the bot's path home or to the enemy flag
        /// if the thing has the right ID, then clear the relevant path if it's not reachable
        /// </summary>
        /// <param name="valuesDict"></param>
        internal void ReceiveCheckReachDetails(Dictionary<string,string> valuesDict)
        {
            Console.Out.WriteLine("in receive_rch_details");

            if (! valuesDict.ContainsKey("ID"))
                return;
                    
            if (valuesDict["ID"] == reachPathHomeId && valuesDict["Reachable"] == "False")
            {
                posInfo.pathHome.Clear();
                Console.Out.WriteLine("Cleared PathHome");
            }
            else if (valuesDict["ID"] == reachPathToEnemyBaseID && valuesDict["Reachable"] == "False")
            {
                posInfo.pathToEnemyBase.Clear();
                Console.Out.WriteLine("Cleared PathToEnemyBase");
            }
        }

        /// <summary>
        /// clean-up after dying
        /// </summary>
        internal void ReceiveDieDetails()
        {
            posInfo.pathHome.Clear();
            posInfo.pathToEnemyBase.Clear();
            posInfo.visitedNavPoints.Clear();
            posInfo.ourFlagInfo.Clear();
            posInfo.enemyFlagInfo.Clear();
        }

        /// <summary>
        /// if the combatinfo class specifies that we need to remain focused on a player, send a relevant strafe command
        /// to move to the provided location.  Otherwise, a runto
        /// </summary>
        /// <param name="?"></param>
        /// <param name="performPrevCheck"></param>
        internal void SendRuntoOrStrafeToLocation(Vector3 location, bool performPrevCheck = true)
        {
            // IMPORTANT-TODO: completely remodel this method as it uses stuff from a different behaviour it should not use.
            Tuple<string,Dictionary<string,string>> message;
            // expire focus id info if necessary FA
            if ( ((Combat)agent.getBehaviour("Combat")).info.KeepFocusOnID is Tuple<string,long> && 
                ((Combat)agent.getBehaviour("Combat")).info.HasFocusIdExpired())
                ((Combat)agent.getBehaviour("Combat")).info.ExpireFocusId();

            //OLD-COMMENT: Seems out of place, need to find a better way of doing this FA
            getBot().SendMessage("STOPSHOOT", new Dictionary<string,string>() ); //no-one to focus on

            if ( ((Combat)agent.getBehaviour("Combat")).info.KeepFocusOnID is Tuple<string,long>)
            {
                message = new Tuple<string,Dictionary<string,string>>("STRAFE",
                    new Dictionary<string,string>() 
                {
                    {"Location", location.ToString()},
                    {"Target", ((Combat)agent.getBehaviour("Combat")).info.KeepFocusOnID.ToString()}
                });
            }
            else
            {
                message = new Tuple<string,Dictionary<string,string>>("RUNTO",
                    new Dictionary<string,string>() 
                {
                    {"Location", location.ToString()},
                });
            }
            if (performPrevCheck)
                getBot().SendIfNotPreviousMessage(message.First,message.Second);
            else
                getBot().SendMessage(message.First,message.Second);

        }

        private POSHBot getBot(string name = "POSHBot")
        {
            return ((POSHBot)agent.getBehaviour(name));
        }

        private void SendGetPath()
        {
            if (_debug_)
                Console.Out.WriteLine("in SendGetPath");

            getBot().SendIfNotPreviousMessage("GETPATH", new Dictionary<string,string>() 
                {
                    // the ID allows us to match requests with answers
                    {"Location", posInfo.ownBasePos.Location.ToString()}, {"Id", pathHomeId}
                });
        }
        /// <summary>
        /// returns 1 and sends a runto message for the provided location 
        /// if the DistanceTolerance check passes otherwise returns 0
        /// </summary>
        /// <param name="location"></param>
        /// <param name="distanceTolerance"></param>
        /// <returns></returns>
        private bool ToKnownLocation(Dictionary<int,Vector3> location, int distanceTolerance)
        {
            if ( location.Count == 0 )
                // even though we failed, we return 1 so that it doesn't tail the list
                return true;
            if (location[0].Distance2DFrom(Vector3.ConvertToVector3(getBot().info["Location"]) ) > distanceTolerance)
            {
                Console.Out.WriteLine("DistanceTolerance check passed");
                Console.Out.WriteLine("About to send RUNTO to");
                Console.Out.WriteLine(location[0].ToString());
                Console.Out.WriteLine("Current location");
                Console.Out.WriteLine(getBot().info["Location"]);

                SendRuntoOrStrafeToLocation(location[0],false);
                return true;
            }
            else
                return false;
        }

        ///
        /// SENSES
        /// 
        
                
        private bool atTargetLocation(NavPoint target, int distanceTolerance)
        {
            if ( !getBot().info.ContainsKey("Location") )
                return false;
            Vector3 location = Vector3.ConvertToVector3(getBot().info["Location"]);

            if (target == null)
                return false;
            else
            {
                // this distance may need adjusting in future (we may also wish to consider the Z axis)
                if (location.DistanceFrom(target.Location) < distanceTolerance)
                    return true;
                else 
                    return false;
            }
        }

        /// <summary>
        /// returns 1 if we're near enough to enemy base
        /// </summary>
        [ExecutableSense("AtEnemyBase")]
        public bool AtEnemyBase()
        {
            return atTargetLocation(posInfo.enemyBasePos,100);
        }

        /// <summary>
        /// returns 1 if we're near enough to our own base
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("AtOwnBase")]
        public bool AtOwnBase()
        {
            return atTargetLocation(posInfo.ownBasePos,10);
        }

        /// <summary>
        /// returns 1 if we have a location for the enemy base
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("KnowEnemyBasePos")]
        public bool KnowEnemyBasePos()
        {
            return (posInfo.enemyBasePos != null) ? true : false; 
        }

        /// <summary>
        /// returns 1 if we have a location for our own base
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("KnowOwnBasePos")]
        public bool KnowOwnBasePos()
        {
            return (posInfo.ownBasePos != null) ? true : false; 
        }

        /// <summary>
        /// returns 1 if there's a reachable nav point in the bot's list which we're not already at
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("ReachableNavPoint")]
        public bool ReachableNavPoint()
        {
            Vector3 location;
            NavPoint navPoint;
            int distanceTolerance = 30; // how near we must be to be thought of as at the nav point

            if ( getBot().info.ContainsKey("Location") )
                location = Vector3.ConvertToVector3(getBot().info["Location"]);
            else
                // if we don't know where we are, treat it as (0,0,0) 
                // as that will just mean we go to the nav point even if we're close by
                location = new Vector3();

            Console.Out.WriteLine(location.ToString());

            // is there already a navpoint we're aiming for?
            
            if (posInfo.chosenNavPoint != null)
            {
                navPoint = posInfo.chosenNavPoint;
                if ( navPoint.DistanceFrom(location) > distanceTolerance )
                    return true;
                else
                {
                    posInfo.visitedNavPoints.Add(navPoint.Id);
                    posInfo.chosenNavPoint = null;
                }
            }
            // now look at the list of navpoints the bot can see
            if ( getBot().navPoints == null || getBot().navPoints.Count() < 1 )
            {
                Console.Out.WriteLine("  no nav points");
                return false;
            }
            else
            {
                // nav_points is a list of tuples.  Each tuple contains an ID and a dictionary of attributes as defined in the API
                // Search for reachable nav points
                List<NavPoint> possibleNPs = new List<NavPoint>(GetReachableNavPoints(getBot().navPoints.Values.ToArray(), distanceTolerance, location));

                // now work through this list of NavPoints until we find once that we haven't been to
                // or the one we've been to least often
                if (possibleNPs.Count == 0)
                {
                    Console.Out.WriteLine("    No PossibleNPs");
                    return false;
                }
                else
                {
                    posInfo.chosenNavPoint = GetLeastOftenVisitedNavPoint(possibleNPs);
                    Console.Out.WriteLine("    Possible NP, returning 1");
                    return true;
                }
            }
        }

        /// <summary>
        /// not well optimised because it also checks each duplicate
        /// </summary>
        /// <param name="navPoints"></param>
        /// <returns></returns>
        internal NavPoint GetLeastOftenVisitedNavPoint(List<NavPoint> navPoints)
        {

            // FIX: corrected mistake in the original implementation which should have faulted the code

            int currentMin = posInfo.visitedNavPoints.Count(g => g == navPoints[0].Id);
            NavPoint currentMinNP = navPoints[0];

            foreach (NavPoint point in navPoints)
            {
                int currentCount = posInfo.visitedNavPoints.Count(g => g == point.Id);
                if (currentCount < currentMin)
                {
                    currentMin = currentCount;
                    currentMinNP = point;
                }
            }

            return currentMinNP;
        }

        internal NavPoint [] GetReachableNavPoints(NavPoint [] navPoints, int distanceTolerance, Vector3 target)
        {
            List<NavPoint> result = new List<NavPoint>();

            foreach (NavPoint currentNP in navPoints )
                if  (currentNP.Reachable && currentNP.Distance2DFrom(target,Vector3.Orientation.XY) > distanceTolerance)
                    result.Add(currentNP);
            
            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>returns 1 if the enemy flag is specified as reachable</returns>
        [ExecutableSense("EnemyFlagReachable")]
        public bool EnemyFlagReachable()
        {
            if (this.posInfo.HasEnemyFlagInfoExpired())
                posInfo.ExpireEnemyFlagInfo();

            // debug
            if (_debug_)
            {
                Console.Out.WriteLine("in EnemyFlagReachable");
                if (posInfo.enemyFlagInfo.Count() > 0)
                    Console.Out.WriteLine(posInfo.enemyFlagInfo.ToString());
            }

            // Made simpler FA
            if (posInfo.enemyFlagInfo.Count() > 0 && bool.Parse(posInfo.enemyFlagInfo["Reachable"]) == true)
                return true;

            return false;
            
        }

                /// <summary>
        /// 
        /// </summary>
        /// <returns>returns 1 if our flag is specified as reachable</returns>
        [ExecutableSense("OurFlagReachable")]
        public bool OurFlagReachable()
        {
            if (this.posInfo.HasOurFlagInfoExpired())
                posInfo.ExpireOurFlagInfo();

            // debug
            if (_debug_)
            {
                Console.Out.WriteLine("in OurFlagReachable");
                if (posInfo.ourFlagInfo.Count() > 0)
                    Console.Out.WriteLine(posInfo.ourFlagInfo.ToString());
            }

            // Made simpler FA
            if (posInfo.ourFlagInfo.Count() > 0 && bool.Parse(posInfo.ourFlagInfo["Reachable"]) == true)
                return true;

            return false;
            
        }

        [ExecutableSense("SeeEnemy")]
        public bool SeeEnemy()
        {
            if (getBot().viewPlayers.Count  == 0 ||  getBot().info.Count == 0)
                return false;
            
            // work through, looking for an enemy
            string ourTeam = getBot().info["Team"];
            UTPlayer [] players = getBot().viewPlayers.Values.ToArray();
            foreach (UTPlayer currentPlayer in players )
                if ( currentPlayer.Team != ourTeam )
                {
                    if (_debug_)
                        Console.Out.WriteLine("SeeEnemy: we can see an enemy!");
                    return true;
                }
            
            return false;
        }

        [ExecutableSense("SeeReachableMedicalKit")]
        public bool SeeReachableMedicalKit()
        {
            if (getBot().viewItems.Count < 1)
                return false;

            // look through items for a Medkit
            foreach (InvItem item in getBot().viewItems)
                if ((item.Class == "Health" || item.Class == "MedBox" ) && item.Reachable )
                    return true;

            return false;
        }

        [ExecutableSense("SeeReachableWeapon")]
        public bool SeeReachableWeapon()
        {
            if (getBot().viewItems.Count < 1)
                return false;

            // look through items for a Medkit
            foreach (InvItem item in getBot().viewItems)
                if ( item.IsKnownWeaponClass() && item.Reachable )
                    return true;

            return false;
        }

        [ExecutableSense("TooCloseForPath")]
        public long TooCloseForPath()
        {
            if ( posInfo.HasTooCloseForPathExpired() )
                posInfo.ExpireTooCloseForPath();

            if (posInfo.TooCloseForPath != 0L )
                Console.Out.WriteLine("we are too close for path");
            return posInfo.TooCloseForPath;
        }

        ///
        /// ACTIONS
        /// 

        [ExecutableAction("RuntoMedicalKit")]
        public bool RuntoMedicalKit()
        {
            if (getBot().viewItems.Count < 1)
                return true;
            
            // look through for a medical kit
            foreach (InvItem item in getBot().viewItems )
                if ( (item.Class == "Health" || item.Class == "MedBox" ) && item.Reachable )
                    SendRuntoOrStrafeToLocation(item.Location);
            
            return true;
        }

        [ExecutableAction("RuntoWeapon")]
        public bool RuntoWeapon()
        {
            if (getBot().viewItems.Count < 1)
                return true;
            
            // look through for a weapon
            foreach (InvItem item in getBot().viewItems )
                if ( item.IsKnownWeaponClass() && item.Reachable )
                {
                    if (_debug_)
                    {
                        Console.Out.WriteLine("in RuntoWeapon");
                        Console.Out.WriteLine(item.Class);
                    }
                    SendRuntoOrStrafeToLocation(item.Location);
                    return true;
                }
            return true;
        }

        /// <summary>
        /// Runs to the chosen Navpoint
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("WalkToNavPoint")]
        public bool WalkToNavPoint()
        {
            if (_debug_)
                Console.Out.WriteLine("in WalkToNavPoint");
            SendRuntoOrStrafeToLocation(posInfo.chosenNavPoint.Location);
            
            return true;
        }

        /// <summary>
        /// runs to the enemy flag
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("ToEnemyFlag")]
        public bool ToEnemyFlag()
        {
            if (_debug_)
                Console.Out.WriteLine("in ToEnemyFlag");
            if (posInfo.HasEnemyFlagInfoExpired())
                posInfo.ExpireEnemyFlagInfo();
            if (posInfo.enemyFlagInfo is Dictionary<string,string>)
                getBot().SendMessage("RUNTO", new Dictionary<string,string>() 
                {
                    {"Target",posInfo.enemyFlagInfo["Id"]}
                });

            return true;
        }

        /// <summary>
        /// runs to own flag
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("ToOwnFlag")]
        public bool ToOwnFlag()
        {
            if (_debug_)
                Console.Out.WriteLine("in ToOwnFlag");
            if (posInfo.HasEnemyFlagInfoExpired())
                posInfo.ExpireEnemyFlagInfo();
            if (posInfo.ourFlagInfo is Dictionary<string,string>)
                SendRuntoOrStrafeToLocation(Vector3.ConvertToVector3(posInfo.ourFlagInfo["Location"]));

            return true;
        }

        private void ToBase(Dictionary<int,Vector3> pathToBase,NavPoint baseNavPoint, string name, string reachId,int distanceTolerance)
        {
            // TODO: method looks incomplete
            if (_debug_)
                Console.Out.WriteLine("in To"+name+"Base");
            // If we don't know where our own base is, then do nothing
            // However, this action should never fire unless we do know where our base is
            if (baseNavPoint == null)
            {
                Console.Out.WriteLine("Don't know where "+name+" base is!");
                return;
            }
            SendGetPath();
            // OLD-COMMENT: if we haven't already got a list of path nodes to follow then send the GETPATH message
            // to try and mitigate the problem of pathhome being cleared part way through this, we assign
            // the relevant value to a variable and then use that throughout, so the array is checked as infrequently as possible
            // it's not an ideal fix though!
            
            if (pathToBase.Count < 1)
                SendGetPath();
            else if ( ! ToKnownLocation(pathToBase,distanceTolerance) )
            {
                Console.Out.WriteLine("DT check failed, tailing");
                pathToBase.Remove(0);
                if (pathToBase.Count > 0)
                {
                    Console.Out.WriteLine("tail not empty");
                    SendRuntoOrStrafeToLocation(pathToBase[0]);
                }
                else
                    SendGetPath();
            }
            // before we return, send a checkreach command about the current navpoint.  That way the list can be recreated if it becomes incorrect
            if (pathToBase is Dictionary<int,Vector3> && pathToBase.Count > 0 )
                getBot().SendMessage("CHECKREACH", new Dictionary<string,string>()
                    {
                        {"Location", pathToBase[0].ToString()}, {"Id",reachId},{"From",getBot().info["Location"]}
                    });
            Console.Out.WriteLine("about to return from To"+name+"Base");
        }


        [ExecutableAction("ToOwnBase")]
        public void ToOwnBase()
        {
            ToBase(posInfo.pathHome,posInfo.ownBasePos,"Own",reachPathHomeId,30);
        }

        [ExecutableAction("ToOwnBase")]
        public void ToEnemyBase()
        {
            ToBase(posInfo.pathToEnemyBase,posInfo.enemyBasePos,"Enemy",reachPathToEnemyBaseID,30);
        }


    //#not used at present (18/2/2005)
    //def inch(self):
    //    # just add a bit to the x value
    //    print "in inch"
    //    (SX, SY, SZ) = utilityfns.location_string_to_tuple(self.agent.Bot.botinfo["Location"])
    //    NewLocTuple = (SX + 150, SY, SZ)
    //    self.send_runto_or_strafe_to_location(utilityfns.location_tuple_to_string(NewLocTuple))
  
    //# just because something was reachable the last time we knew about it doesn't mean it still is
    //#def expire_reachable_info(self):
    //#    if self.PosInfo.OurFlagInfo != {} and self.PosInfo.OurFlagInfo.has_key("Reachable"):
    //#        self.PosInfo.OurFlagInfo["Reachable"] = "0"
    //#    if self.PosInfo.EnemyFlagInfo != {} and self.PosInfo.EnemyFlagInfo.has_key("Reachable"):
    //#        self.PosInfo.EnemyFlagInfo["Reachable"] = "0"
    //#    
    //#    self.PosInfo.TooCloseForPath = 0
    

                
    
            }

    
}