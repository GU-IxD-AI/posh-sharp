using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Posh_sharp.examples.BODBot.util;

namespace Posh_sharp.BODBot.util
{
    public class Projectile
    {
        protected internal Vector3 Velocity { get; internal set;}
        protected internal float Speed { get; internal set; }
        protected internal Vector3 Location { get; internal set; }
        
        /// <summary>
        /// Time until impact
        /// </summary>
        protected internal int Time { get; internal set; }
        protected internal Vector3 Direction { get; internal set; }
        protected internal Vector3 Origin { get; internal set; }
        protected internal float DamageRadius { get; internal set; }
        protected internal string Type { get; internal set; }

        protected internal int TimeStamp { get; internal set; }

        public Projectile(Dictionary<string,string> dictRaw)
        {
            Velocity = Vector3.ConvertToVector3(dictRaw["Velocity"]);
            Speed = float.Parse(dictRaw["Speed"]);
            Location = Vector3.ConvertToVector3(dictRaw["Location"]);
            Time = int.Parse(dictRaw["Time"]);
            Direction = Vector3.ConvertToVector3(dictRaw["Direction"]);
            Origin = Vector3.ConvertToVector3(dictRaw["Origin"]);
            DamageRadius = float.Parse(dictRaw["DamageRadius"]);
            Type = dictRaw["Type"];
            TimeStamp = int.Parse(dictRaw["TimeStamp"]);
            

        }
        



    }
}
