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
        internal UTPlayer HoldingOurFlagPlayerInfo;
        
        internal Projectile ProjectileDetails;
        internal Damage DamageDetails;
        internal Tuple<string,long> KeepFocusOnID;
        internal Tuple<Vector3,long> KeepFocusOnLocation;

        public CombatInfo()
        {
            HoldingOurFlag = null;
            HoldingOurFlagPlayerInfo = null;
            ProjectileDetails = null;
            DamageDetails = null;
            KeepFocusOnID = null;
            KeepFocusOnLocation = null;
        }

        /// <summary>
        /// Checks the timestamp against current time less lifetime of damagedetails FA
        /// If the details expired null is returned.
        /// </summary>
        /// <param name="lsec"></param>
        /// <returns>damage</returns>
        public Damage GetDamageDetails(int lsec = 5)
        {
            if (DamageDetails != null && DamageDetails.TimeStamp < TimerBase.CurrentTimeStamp() - lsec )
                return DamageDetails;

            DamageDetails = null;
            return null;
        }

        /// <summary>
        /// Checks the timestamp against current time less lifetime of focus_id FA
        /// If the focus ID expired null is returned.
        /// 
        /// </summary>
        /// <param name="lsec"></param>
        /// <returns></returns>
        public Tuple<string,long> GetFocusId(int lsec = 15)
        {
            if (KeepFocusOnID is Tuple<string,long> && KeepFocusOnID.First != string.Empty )
                if (KeepFocusOnID.Second < TimerBase.CurrentTimeStamp() - lsec)
                    return KeepFocusOnID;

            KeepFocusOnID = null;
            return null;

        }

        /// <summary>
        /// Checks the timestamp against current time less lifetime of focus_id FA
        /// If the focus location expired null is returned.
        /// </summary>
        /// <param name="lsec"></param>
        /// <returns></returns>
        public Tuple<Vector3,long> GetFocusLocation(int lsec = 15)
        {
            if (KeepFocusOnLocation is Tuple<Vector3,long> && KeepFocusOnLocation.First is Vector3 )
                if (KeepFocusOnLocation.Second < TimerBase.CurrentTimeStamp() - lsec)
                    return KeepFocusOnLocation;

            KeepFocusOnLocation = null;
            return null;
        }

        public Projectile GetProjectileDetails(int lsecs = 2)
        {
            if (ProjectileDetails != null && ProjectileDetails.TimeStamp < TimerBase.CurrentTimeStamp() - lsecs)
                return ProjectileDetails;

            ProjectileDetails = null;
            return null;
        }
    }
}