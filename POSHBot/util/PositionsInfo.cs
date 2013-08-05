using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys;
using POSH_sharp.sys.strict;

namespace Posh_sharp.POSHBot.util
{
    /// <summary>
    /// stores details about where things are
    /// </summary>
    public class PositionsInfo
    {
        protected internal NavPoint ownBasePos { internal set; get; }
        protected internal NavPoint enemyBasePos { internal set; get; }

        /// <summary>
        /// contains an ordered list of all visited NavPoint Ids
        /// </summary>
        protected internal  List<string> visitedNavPoints;
        protected internal NavPoint chosenNavPoint;
        protected internal Dictionary<string, string> ourFlagInfo;
        protected internal Dictionary<string,string> enemyFlagInfo;

        protected internal Dictionary<int, Vector3> pathHome;
        protected internal Dictionary<int, Vector3> pathToEnemyBase;

        /// <summary>
        /// set to current time if we're sent a blank path
        /// Blank paths indicate that we're right next to something but can't actually see it
        /// </summary>
        public long TooCloseForPath { get; internal set; }

        public PositionsInfo()
        {
            ownBasePos = null;
            enemyBasePos = null;
            visitedNavPoints = new List<string>();
            chosenNavPoint = null;
            ourFlagInfo = new Dictionary<string,string>();
            enemyFlagInfo = new Dictionary<string,string>();

            pathHome = new Dictionary<int, Vector3>();
            pathToEnemyBase = new Dictionary<int, Vector3>();

            TooCloseForPath = 0L;
        }

		public bool HasEnemyFlagInfoExpired()
		{
			return HasEnemyFlagInfoExpired (10);
		}

        public bool HasEnemyFlagInfoExpired(int lsecs)
        {
            if (enemyFlagInfo.Count > 0 && enemyFlagInfo.ContainsKey("Reachable"))
                if (long.Parse(enemyFlagInfo["timestamp"]) < ( TimerBase.CurrentTimeStamp() - lsecs) )
                    return true;
            
            return false;
        }

        /// <summary>
        /// Have to call check_enemy_flag_info_expired before calling this FA 
        /// </summary>
        public void ExpireEnemyFlagInfo()
        {
            enemyFlagInfo["Reachable"] = false.ToString();
        }

		public bool HasOurFlagInfoExpired()
		{
			return HasOurFlagInfoExpired (10);
		}

        public bool HasOurFlagInfoExpired(int lsecs)
        {
            if ( ourFlagInfo.Count > 0 && ourFlagInfo.ContainsKey("Reachable") )
                if (long.Parse(ourFlagInfo["timestamp"]) < ( TimerBase.CurrentTimeStamp() - lsecs) )
                    return true;
            
            return false;
        }

        /// <summary>
        /// Have to call check_our_flag_info_expired before calling this FA 
        /// </summary>
        public void ExpireOurFlagInfo()
        {
            ourFlagInfo["Reachable"] = false.ToString();
        }

		public bool HasTooCloseForPathExpired()
		{
			return HasTooCloseForPathExpired (10);
		}

        public bool HasTooCloseForPathExpired(int lsecs)
        {
            if ( TooCloseForPath < TimerBase.CurrentTimeStamp() - lsecs )
                return true;

            return false;
        }

        public void ExpireTooCloseForPath()
        {
            TooCloseForPath = 0L;
        }
    }
}

