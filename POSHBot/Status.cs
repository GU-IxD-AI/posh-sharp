using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys;
using POSH_sharp.sys.annotations;
using Posh_sharp.POSHBot.util;

namespace Posh_sharp.POSHBot
{
    /// <summary>
    /// The status behaviour has primitives for stuff to do with finding out
    /// the bot's state (e.g. amount of health).
    /// </summary>
    public class Status : UTBehaviour
    {

		public Status(AgentBase agent) : base(agent,
                        new string[] {},
		new string[] {"fail","succeed","focusing_task","game_ended","have_enemy_flag"})
        {}

		/*
         * 
         * Internal
         * 
         */

		/*
         * 
         * ACTIONS
         * 
         */

		// None at this point


        /*
         * 
         * SENSES
         * 
         */
        [ExecutableSense("fail")]
        public bool fail()
        {
            return false;
        }

        [ExecutableSense("succeed")]
        public bool succeed()
        {
            return true;
        }

		[ExecutableSense("game_ended")]
		public bool game_ended()
		{
            if (_debug_)
                Console.Out.WriteLine("in game_ended");
			if (GetBot ().killConnection)
				return true;
			return false;
		}



		[ExecutableSense("focusing_task")]
		public bool focusing_task()
		{
            if (_debug_)
                Console.Out.WriteLine("in focusing_task");
            if (have_enemy_flag())
                return true;

			return false;
		}

        [ExecutableSense("have_enemy_flag")]
		public bool have_enemy_flag()
        {
            if (_debug_)
                Console.Out.WriteLine("in have_enemy_flag");

            if (GetCombat().info.HoldingEnemyFlag != null && GetCombat().info.HoldingEnemyFlag == GetBot().info["BotId"])
            {
                return true;
            }

            return false;
        }

        [ExecutableSense("OwnHealthLevel")]
        public string OwnHealthLevel()
        {
            return GetBot().info["Health"];
        }

        [ExecutableSense("AreArmed")]
        public bool AreArmed()
        {
            if (GetBot().info.Count == 0)
                return false;

            if (GetBot().info["Weapon"] == "None")
            {
                Console.WriteLine("unarmed");
                return false;
            }
            Console.WriteLine("armed with "+GetBot().info["Weapon"]);

            return true;
        }

        [ExecutableSense("AmmoAmount")]
        public int AmmoAmount()
        {
            if (GetBot().info.Count == 0)
                return 0;

            return int.Parse(GetBot().info["CurrentAmmo"]);
        }

        [ExecutableSense("ArmedAndAmmo")]
        public bool ArmedAndAmmo()
        {
            return (AreArmed() && AmmoAmount() > 0) ? true : false;
        }

        
    }
}
