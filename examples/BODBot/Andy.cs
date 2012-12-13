using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys;
using POSH_sharp.sys.annotations;
using Posh_sharp.examples.BODBot.util;

namespace Posh_sharp.examples.BODBot
{
    public class Andy : Behaviour
    {
        public Andy(AgentBase agent)
            : base(agent, new string[] {"StopBot",
                            "Rotate", "Walk",
                            "BigRotate", "MovePlayer", "PickupItem"},
                        new string[] {"SeeEnemyWithOurFlag",
                            "SeePlayer", "SeeItem",
                            "CloseToPlayer", "IsRotating",
                            "HitObject", "IsWalking", "IsStuck",
                            "Fail", "Succeed"})
        {
            // senses fail and succeed
            // self.fail = lambda : False
            // self.succeed = lambda : True
        }
        private BODBot getBot(string name="Bot")
        {
            return ((BODBot)agent.getBehaviour("Bot"));
        }
        /*
         * 
         * SENSES
         * 
         */

        [ExecutableSense("SeePlayer")]
        public bool SeePlayer()
        {
            if (getBot().viewPlayers.Count > 0)
                return true;

            return false;
        }

        [ExecutableSense("CloseToPlayer")]
        public bool CloseToPlayer()
        {
            // print "Checking to see if close to player..."
            // If we are really close to the first player
            int closeness = 50;
            
            if (getBot().viewPlayers.Count > 0)
                if (getBot().viewPlayers.First().Value.Location.Distance2DFrom(Vector3.ConvertToVector3(getBot().info["Location"]),Vector3.Orientation.XY) < closeness)
                    return true;
                
            return false;
        }

        [ExecutableSense("SeeItem")]
        public bool SeeItem()
        {
            // print "See an item?"
            
            if (getBot().viewItems.Count > 0)
                return true;
                
            return false;
        }

        [ExecutableSense("HitObject")]
        public bool HitObject()
        {
            // print "Was I hit?"
            if (getBot().WasHit() > 0)
                return true;
                
            return false;
        }

    }
}

    #  == SENSES ==
            


    def hit_object(self):
        #print "Was I hit?"
        if self.agent.Bot.was_hit():
            #print "yes!!!"
            return 1
        else:
            #print "Ha, got you"
            return 0

    def is_rotating(self):
        #print "is_rotating"
        return self.agent.Bot.turning()

    def is_walking(self):
        return self.agent.Bot.moving()

    def is_stuck(self):
        #print "is stuck?"
        return self.agent.Bot.stuck()
        
    
    #  == ACTIONS ==
    
    def move_player(self):
        #print "Move to player..."
        # Find the first player and move to it
        players = self.agent.Bot.view_players.values()
        
        if len(players) > 0:
            id = players[0]["Id"]
        #    print str(id)
            self.agent.Bot.send_message("RUNTO", {"Target" : str(id)})
            
        return 1

    def pickup_item(self):
        #print "Pickup item..."
        # Pickup the first item on the list
        items = self.agent.Bot.view_items.values()
        
        if len(items) > 0:
            id = items[0]["Id"]           
            self.agent.Bot.send_message("RUNTO", {"Target": str(id)})
        return 1

    def rotate(self, angle = None):
        #print "Rotating..."
        def turnleft():
            self.agent.Bot.turn(90)
            
        def turnright():
            self.agent.Bot.turn(-90)
            
        actions = (turnleft, turnright)
        if angle == None:
            random.choice(actions)() # Run the action randomly
        else:
            self.agent.Bot.turn(angle)
            # was self.agent.Bot.move(angle) but this is silly
        return 1
        
    # rotates by 160 degrees.  Should allow quick exploring of places without getting stuck at edges etc
    def big_rotate(self):
        #print "big rotate"
        angle = 160
        self.rotate(angle)
        return 1

    def walk(self):
        #print "Walking..."
        return self.agent.Bot.move()

    def stop_bot(self):
        #print "Stopping Bot"
        self.agent.Bot.send_message("STOP", {})
        return 1
