using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys;

namespace Posh_sharp.examples.BODBot
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
                            "AmmoAmount", "ArmedAndAmmo"})
        {}

        private BODBot getBot(string name="Bot")
        {
            return ((BODBot)agent.getBehaviour("Bot"));
        }
    }
}

    
    # === SENSES ===
    
    # returns 1 if we are carrying the enemy's flag
    def have_enemy_flag(self):
        #print "have_enemy_flag?"
        if not self.agent.Bot.gameinfo.has_key("HaveFlag"):
            return 0
        else:
            #print "have enemy flag!"
            return 1
            
    def own_health_level(self):
        HealthLevel = int(self.agent.Bot.botinfo["Health"])
        #print "Our bot has health ",
        #print HealthLevel
        return HealthLevel
        
    def are_armed(self):
        if self.agent.Bot.botinfo == {}:
            return 0
        else:
            if self.agent.Bot.botinfo["Weapon"] == "None":
                print "unarmed",
                print self.agent.Bot.botinfo["Weapon"]
                return 0
            else:
                print "armed",
                print self.agent.Bot.botinfo["Weapon"]
                return 1
        
    def ammo_amount(self):
        if self.agent.Bot.botinfo == {}:
            return 0
        else:
            return int(self.agent.Bot.botinfo["CurrentAmmo"])
            
    def armed_and_ammo(self):
        #return 1
        return (self.are_armed()) and (self.ammo_amount() > 0)
        
    def check_error(self):
        return 0
        
    # use have_enemy_flag instead
    # this method won't work anymore as there is no posinfo -> get it from other behaviours
    #def holding_enemy_flag(self):
    #    if self.PosInfo == None or self.PosInfo.EnemyFlagInfo == {}:
    #        return 0
    #   elif self.PosInfo.EnemyFlagInfo["State"] == "held" and self.PosInfo.EnemyFlagInfo["Holder"] == self.agent.Bot.bot_info["Id"]:
    #        return 1
    #    else:
    #        return 0
            
    # === ACTIONS ===
    
    # none at present
