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
    public class Movement : UTBehaviour
    {
        internal PositionsInfo posInfo;
        string pathHomeId;
        string reachPathHomeId;
        string pathToEnemyBaseId;
        string reachPathToEnemyBaseID;

        public Movement(AgentBase agent)
            : base(agent,
            new string[] { "MoveToNavpoint","StopBot", "Idleing", "WalkToNavPoint", "Rotate","BigRotate" },
            new string[] {"CloseNavpoint","AtEnemyBase", "AtOwnBase", "KnowEnemyBasePos",
                            "KnowOwnBasePos", "ReachableNavPoint",
                            "EnemyFlagReachable", "OurFlagReachable",
                            "SeeEnemy","IsRotating", "IsWalking", "IsStuck" })
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
        internal void ReceiveFlagDetails(Dictionary<string, string> values)
        {
            // TODO: fix the mix of information in this method it should just contain relevant info

            Console.Out.WriteLine("in receiveFlagDetails");
            Console.Out.WriteLine(values.ToArray().ToString());

            if (getBot().info == null || getBot().info.Count < 1)
                return;
            // set flag stuff
            if (values["Team"] == getBot().info["Team"])
            {
                if (posInfo.ourFlagInfo != null && posInfo.ourFlagInfo.ContainsKey("Location"))
                    return;
                posInfo.ourFlagInfo = values;
            }
            else
            {
                if (posInfo.enemyFlagInfo != null && posInfo.enemyFlagInfo.ContainsKey("Location"))
                    return;
                posInfo.enemyFlagInfo = values;
            }

            if (values["State"] == "home")
                if (values["Team"] == getBot().info["Team"])
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
        internal void ReceivePathDetails(Dictionary<string, string> valuesDict)
        {
            if (!valuesDict.ContainsKey("ID"))
                return;
            if (valuesDict["ID"] == pathHomeId)
                posInfo.pathHome = NavPoint.ConvertToPath(valuesDict);
            else if (valuesDict["ID"] == pathToEnemyBaseId)
                posInfo.pathToEnemyBase = NavPoint.ConvertToPath(valuesDict);

            // if there's no 0 key we're being given an empty path, so set TooCloseForPath to the current time
            // so that we can check how old the information is later on
            if (!valuesDict.ContainsKey("0"))
                posInfo.TooCloseForPath = TimerBase.CurrentTimeStamp();
            else
                posInfo.TooCloseForPath = 0L;
        }

        /// <summary>
        /// used in validating the bot's path home or to the enemy flag
        /// if the thing has the right ID, then clear the relevant path if it's not reachable
        /// </summary>
        /// <param name="valuesDict"></param>
        internal void ReceiveCheckReachDetails(Dictionary<string, string> valuesDict)
        {
            Console.Out.WriteLine("in receive_rch_details");

            if (!valuesDict.ContainsKey("ID"))
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
        internal void ReceiveDeathDetails()
        {
            posInfo.pathHome.Clear();
            posInfo.pathToEnemyBase.Clear();
            posInfo.visitedNavPoints.Clear();
            posInfo.ourFlagInfo.Clear();
            posInfo.enemyFlagInfo.Clear();
        }
		internal void SendMoveToLocation(Vector3 location)
		{ 
			SendMoveToLocation (location, true);
		}
        /// <summary>
        /// if the combatinfo class specifies that we need to remain focused on a player, send a relevant strafe command
        /// to move to the provided location.  Otherwise, a runto
        /// </summary>
        /// <param name="?"></param>
        /// <param name="performPrevCheck"></param>
        internal void SendMoveToLocation(Vector3 location, bool performPrevCheck)
        {
            // IMPORTANT-TODO: completely remodel this method as it uses stuff from a different behaviour it should not use.
            Tuple<string, Dictionary<string, string>> message;
            // expire focus id info if necessary FA
            
            if (getCombat().info.GetFocusId() is Tuple<string, long>)
            {
                message = new Tuple<string, Dictionary<string, string>>("MOVE",
                    new Dictionary<string, string>() 
                {
                    {"FirstLocation", location.ToString()},
                    {"FocusTarget", getCombat().info.KeepFocusOnID.ToString()}
                });
            }
            else
            {
                getBot().SendMessage("STOPSHOOT", new Dictionary<string, string>());
                message = new Tuple<string, Dictionary<string, string>>("MOVE",
                    new Dictionary<string, string>() 
                {
                    {"FirstLocation", location.ToString()},
                });
            }
            if (performPrevCheck)
                getBot().SendIfNotPreviousMessage(message.First, message.Second);
            else
                getBot().SendMessage(message.First, message.Second);

        }

        private void SendGetPath()
        {
            if (_debug_)
                Console.Out.WriteLine("in SendGetPath");

            getBot().SendIfNotPreviousMessage("GETPATH", new Dictionary<string, string>() 
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
        private bool ToKnownLocation(Dictionary<int, Vector3> location, int distanceTolerance)
        {
            if (location.Count == 0)
                // even though we failed, we return 1 so that it doesn't tail the list
                return true;
            if (location[0].Distance2DFrom(Vector3.ConvertToVector3(getBot().info["Location"]),Vector3.Orientation.XY) > distanceTolerance)
            {
                Console.Out.WriteLine("DistanceTolerance check passed");
                Console.Out.WriteLine("About to send MOVE to");
                Console.Out.WriteLine(location[0].ToString());
                Console.Out.WriteLine("Current location");
                Console.Out.WriteLine(getBot().info["Location"]);

                SendMoveToLocation(location[0], false);
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
            if (!getBot().info.ContainsKey("Location"))
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

        /// <summary>
        /// returns 1 if we're near enough to enemy base
        /// </summary>
        [ExecutableSense("AtEnemyBase")]
        public bool AtEnemyBase()
        {
            return atTargetLocation(posInfo.enemyBasePos, 100);
        }

        /// <summary>
        /// returns 1 if we're near enough to our own base
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("AtOwnBase")]
        public bool AtOwnBase()
        {
            return atTargetLocation(posInfo.ownBasePos, 10);
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

            if (getBot().info.ContainsKey("Location"))
                location = Vector3.ConvertToVector3(getBot().info["Location"]);
            else
                // if we don't know where we are, treat it as (0,0,0) 
                // as that will just mean we go to the nav point even if we're close by
                location = Vector3.NullVector();

            Console.Out.WriteLine(location.ToString());

            // is there already a navpoint we're aiming for?

            if (posInfo.chosenNavPoint != null)
            {
                navPoint = posInfo.chosenNavPoint;
                if (navPoint.DistanceFrom(location) > distanceTolerance)
                    return true;
                else
                {
                    posInfo.visitedNavPoints.Add(navPoint.Id);
                    posInfo.chosenNavPoint = null;
                }
            }
            // now look at the list of navpoints the bot can see
            if (getBot().navPoints == null || getBot().navPoints.Count() < 1)
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

        internal NavPoint[] GetReachableNavPoints(NavPoint[] navPoints, int distanceTolerance, Vector3 target)
        {
            List<NavPoint> result = new List<NavPoint>();

            foreach (NavPoint currentNP in navPoints)
                if (currentNP.Reachable && currentNP.Distance2DFrom(target, Vector3.Orientation.XY) > distanceTolerance)
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
            if (getBot().viewPlayers.Count == 0 || getBot().info.Count == 0)
                return false;

            // work through, looking for an enemy
            string ourTeam = getBot().info["Team"];
            UTPlayer[] players = getBot().viewPlayers.Values.ToArray();
            foreach (UTPlayer currentPlayer in players)
                if (currentPlayer.Team != ourTeam)
                {
                    if (_debug_)
                        Console.Out.WriteLine("SeeEnemy: we can see an enemy!");
                    return true;
                }

            return false;
        }

        

        ///
        /// ACTIONS
        /// 

        [ExecutableAction("MoveToNavpoint",1f)]
        public bool MoveToNavpoint()
        {
            throw new NotImplementedException();

            
        }

        /// <summary>
        /// Stops the Bot from doing stuff
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("StopBot")]
        public bool StopBot()
        {
            // print "Stopping Bot"
            getBot().SendMessage("STOP", new Dictionary<string, string>());

            return true;
        }

        /// <summary>
        /// Idleing
        /// </summary>
        /// <returns></returns>
        [ExecutableAction("Idleing")]
        public bool Idleing()
        {
            // print "Idleing ..."

            return getBot().Inch();
        }

        /// <summary>
        /// Rotating the bot around itself (left or right)
        /// </summary>
        /// <param name="angle">If angle is not given random 90 degree turn is made</param>
        /// <returns></returns>
        [ExecutableAction("Rotate")]
        public bool Rotate()
        {
            return Rotate(0);
        }

        protected bool Rotate(int angle)
        {
            // print "Rotate ..."
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

            return Rotate(160);
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
            SendMoveToLocation(posInfo.chosenNavPoint.Location);

            return true;
        }

    }
    
}