using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Posh_sharp.BODBot.util
{
    class Damage
    {
        protected internal int Amount { get; internal set; }
        protected internal string Type { get; internal set; }
        protected internal string AttackerID { get; internal set; }

        protected internal int TimeStamp { get; internal set; }

        public Damage(Dictionary<string,string>dictRaw)
        {
            Amount = int.Parse(dictRaw["Damage"]);
            Type = dictRaw["DamageType"];
            AttackerID = (dictRaw.ContainsKey("Instigator")) ? dictRaw["Instigator"] : string.Empty;
            TimeStamp = int.Parse(dictRaw["TimeStamp"]);
        }
    }
}
