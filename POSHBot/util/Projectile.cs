using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Posh_sharp.POSHBot.util
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
            Velocity = dictRaw.ContainsKey("Velocity") ? Vector3.ConvertToVector3(dictRaw["Velocity"]) : null;
            Speed = dictRaw.ContainsKey("Speed") ? float.Parse(dictRaw["Speed"]) : 0;
            Location = dictRaw.ContainsKey("Location") ? Vector3.ConvertToVector3(dictRaw["Location"]) : null;
            Time = dictRaw.ContainsKey("Time") ? int.Parse(dictRaw["Time"]) : 0;
            Direction = dictRaw.ContainsKey("Direction") ? Vector3.ConvertToVector3(dictRaw["Direction"]) : null;
            Origin = dictRaw.ContainsKey("Origin") ? Vector3.ConvertToVector3(dictRaw["Origin"]) : null;
            DamageRadius = dictRaw.ContainsKey("DamageRadius") ? float.Parse(dictRaw["DamageRadius"]) : 0;
            Type = dictRaw.ContainsKey("Type") ? dictRaw["Type"] : "";
            TimeStamp = dictRaw.ContainsKey("TimeStamp") ? int.Parse(dictRaw["TimeStamp"]): 0;
            

        }
        



    }
}
