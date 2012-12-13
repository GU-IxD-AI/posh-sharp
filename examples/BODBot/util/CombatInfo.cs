using System;
using System.Collections.Generic;
using System.Text;
using Posh_sharp.examples.BODBot.util;
using Posh_sharp.BODBot.util;
using POSH_sharp.sys.strict;
using POSH_sharp.sys;

namespace Posh_sharp.examples.BODBot.util
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
        /// </summary>
        /// <param name="lsec"></param>
        public bool HasDamageInfoExpired(int lsec = 5)
        {
            if (DamageDetails != null && DamageDetails.TimeStamp < TimerBase.CurrentTimeStamp() - lsec )
                return true;
            return false;
        }

        /// <summary>
        /// not the usual sort of action, but ensures that details about e.g. damage taken doesn't reside forever and 
        /// inform decisions too far into the future
        /// </summary>
        public bool ExpireDamageInfo()
        {
            DamageDetails = null;
            return true;
        }

        /// <summary>
        /// Checks the timestamp against current time less lifetime of focus_id FA
        /// </summary>
        /// <param name="lsec"></param>
        /// <returns></returns>
        public bool HasFocusIdExpired(int lsec = 15)
        {
            if (KeepFocusOnID is Tuple<string,long> && KeepFocusOnID.First != string.Empty )
                if (KeepFocusOnID.Second < TimerBase.CurrentTimeStamp() - lsec)
                    return true;
            return false;

        }

        /// <summary>
        /// Split expire_focus_info in to two methods for better accuracy FA
        /// </summary>
        /// <param name="lsec"></param>
        public void ExpireFocusId(int lsec = 15)
        {
            KeepFocusOnID = null;
        }

        /// <summary>
        /// Checks the timestamp against current time less lifetime of focus_id FA
        /// </summary>
        /// <param name="lsec"></param>
        /// <returns></returns>
        public bool HasFocusLocationExpired(int lsec = 15)
        {
            if (KeepFocusOnLocation is Tuple<Vector3,long> && KeepFocusOnLocation.First is Vector3 )
                if (KeepFocusOnLocation.Second < TimerBase.CurrentTimeStamp() - lsec)
                    return true;
            return false;
        }

        /// <summary>
        /// Split expire_focus_info in to two methods for better accuracy FA
        /// </summary>
        public void ExpireFocusLocation()
        {
            KeepFocusOnLocation = null;
        }

        public bool HasProjectileDetailsExpired(int lsecs = 2)
        {
            if (ProjectileDetails != null && ProjectileDetails.TimeStamp < TimerBase.CurrentTimeStamp() - lsecs)
                return true;
            return false;
        }

        public void ExpireProjectileInfo()
        {
            ProjectileDetails = null;
        }

    }
}