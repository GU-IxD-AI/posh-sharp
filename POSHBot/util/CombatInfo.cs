using System;
using System.Collections.Generic;
using System.Text;
using Posh_sharp.POSHBot.util;
using POSH_sharp.sys.strict;
using POSH_sharp.sys;

namespace Posh_sharp.POSHBot.util
{
    public class CombatInfo
    {
        /// <summary>
        /// the ID of the player holding our flag
        /// </summary>
        internal string HoldingOurFlag;
        internal string HoldingEnemyFlag;
        internal UTPlayer HoldingOurFlagPlayerInfo;
        internal UTPlayer HoldingEnemyFlagPlayerInfo;
        internal Projectile ProjectileDetails;
        internal Damage DamageDetails;
        internal Tuple<string,long> KeepFocusOnID;
        internal Tuple<Vector3,long> KeepFocusOnLocation;

        public CombatInfo()
        {
            HoldingOurFlag = null;
            HoldingEnemyFlag = null;
            HoldingOurFlagPlayerInfo = null;
            HoldingEnemyFlagPlayerInfo = null;
            ProjectileDetails = null;
            DamageDetails = null;
            KeepFocusOnID = null;
            KeepFocusOnLocation = null;
        }

		public Damage GetDamageDetails()
		{
			return GetDamageDetails (5);
		}

        /// <summary>
        /// Checks the timestamp against current time less lifetime of damagedetails FA
        /// If the details expired null is returned.
        /// </summary>
        /// <param name="lsec"></param>
        /// <returns>damage</returns>
        public Damage GetDamageDetails(int lsec)
        {
            if (DamageDetails != null && DamageDetails.TimeStamp < TimerBase.CurrentTimeStamp() - lsec )
                return DamageDetails;

            DamageDetails = null;
            return null;
        }

		public Tuple<string,long> GetFocusId()
		{
			return GetFocusId (15);
		}

        /// <summary>
        /// Checks the timestamp against current time less lifetime of focus_id FA
        /// If the focus ID expired null is returned.
        /// 
        /// </summary>
        /// <param name="lsec"></param>
        /// <returns></returns>
        public Tuple<string,long> GetFocusId(int lsec)
        {
            if (KeepFocusOnID is Tuple<string,long> && KeepFocusOnID.First != string.Empty )
                if (KeepFocusOnID.Second < TimerBase.CurrentTimeStamp() - lsec)
                    return KeepFocusOnID;

            KeepFocusOnID = null;
            return null;

        }
		public Tuple<Vector3,long> GetFocusLocation()
		{
			return GetFocusLocation (15);
		}
        /// <summary>
        /// Checks the timestamp against current time less lifetime of focus_id FA
        /// If the focus location expired null is returned.
        /// </summary>
        /// <param name="lsec"></param>
        /// <returns></returns>
        public Tuple<Vector3,long> GetFocusLocation(int lsec)
        {
            if (KeepFocusOnLocation is Tuple<Vector3,long> && KeepFocusOnLocation.First is Vector3 )
                if (KeepFocusOnLocation.Second < TimerBase.CurrentTimeStamp() - lsec)
                    return KeepFocusOnLocation;

            KeepFocusOnLocation = null;
            return null;
        }
		public Projectile GetProjectileDetails()
		{
			return GetProjectileDetails (2);
		}

        public Projectile GetProjectileDetails(int lsecs)
        {
            if (ProjectileDetails != null && ProjectileDetails.TimeStamp < TimerBase.CurrentTimeStamp() - lsecs)
                return ProjectileDetails;

            ProjectileDetails = null;
            return null;
        }
    }
}