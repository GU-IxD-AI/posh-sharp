using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys;
using POSH_sharp.sys.annotations;

namespace Posh_sharp.POSHBot
{
    /// <summary>
    /// The status behaviour has primitives for stuff to do with finding out
    /// the bot's state (e.g. amount of health).
    /// </summary>
    public class Status : Behaviour
    {
        public Status(AgentBase agent) : base(agent,
                        new string[] {},
                        new string[] {"HaveEnemyFlag",
                            "OwnHealthLevel", "AreArmed",
                            "AmmoAmount", "ArmedAndAmmo","Fail","Succeed"})
        {}

        private POSHBot getBot(string name="POSHBot")
        {
            return ((POSHBot)agent.getBehaviour(name));
        }

        /*
         * 
         * SENSES
         * 
         */
        [ExecutableSense("Fail")]
        public bool Fail()
        {
            return false;
        }

        [ExecutableSense("Succeed")]
        public bool Succeed()
        {
            return true;
        }
        [ExecutableSense("HaveEnemyFlag")]
        public bool HaveEnemyFlag()
        {
            if (getBot().info.ContainsKey("HaveFlag"))
                return true;

            return false;
        }

        [ExecutableSense("OwnHealthLevel")]
        public string OwnHealthLevel()
        {
            return getBot().info["Health"];
        }

        [ExecutableSense("AreArmed")]
        public bool AreArmed()
        {
            if (getBot().info.Count == 0)
                return false;

            if (getBot().info["Weapon"] == "None")
            {
                Console.WriteLine("unarmed");
                return false;
            }
            Console.WriteLine("armed with "+getBot().info["Weapon"]);

            return true;
        }

        [ExecutableSense("AmmoAmount")]
        public int AmmoAmount()
        {
            if (getBot().info.Count == 0)
                return 0;

            return int.Parse(getBot().info["CurrentAmmo"]);
        }

        [ExecutableSense("ArmedAndAmmo")]
        public bool ArmedAndAmmo()
        {
            return (AreArmed() && AmmoAmount() > 0) ? true : false;
        }

        /*
         * 
         * ACTIONS
         * 
         */

        // None at this point
    }
}
