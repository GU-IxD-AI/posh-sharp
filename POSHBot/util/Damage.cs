using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Posh_sharp.POSHBot.util
{
    class Damage
    {
        protected internal int Amount { get; internal set; }
        protected internal string Type { get; internal set; }
        protected internal string AttackerID { get; internal set; }

        protected internal long TimeStamp { get; internal set; }

        public Damage(Dictionary<string,string>dictRaw)
        {
            Amount = int.Parse(dictRaw["Damage"]);
            Type = dictRaw["DamageType"];
            AttackerID = (dictRaw.ContainsKey("Instigator")) ? dictRaw["Instigator"] : string.Empty;
            TimeStamp = long.Parse(dictRaw["TimeStamp"]);
        }
    }
}
