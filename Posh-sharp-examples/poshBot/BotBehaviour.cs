using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.test.examples.poshbot
{
    class BotBehaviour
    {
    }
}
//#
//# The behavior class, we can merge this with the bot class
//# if we really wanted too, but keep it separate for now so that
//# we can someday rip out the gamebots class for other stuff.

//class Behavior(Base):
//    def __init__(self, **kw):
//        Base.__init__(self, **kw) # Call the ancestor init
//        self.act_dict = {}
//        self.sense_dict = {}
//        self.init_acts()
//        self.init_senses()
//        # These are behavior varibles

//    # This method is called by agent.execute in posh_agent to make sure
//    # that the behavior is ok every cycle. Returns 0 if everything is OK.
//    # We can assign error codes or something similar.
//    def check_error(self):
//        if self.bot.conn_ready: # Check the bot to see if connection
//            return 0
//        else:
//            return 1
        
//    # The agent has recieved a request for exit. Stop running everything.
//    def exit_prepare(self):
//        self.bot.disconnect()

//    def init_acts(self):
//        self.add_act("stop-bot", self.stop_bot)
//        self.add_act("rotate", self.rotate)
//        self.add_act("move-player", self.move_player)
//        self.add_act("pickup-item", self.pickup_item)
//        self.add_act("walk", self.walk)

//    def init_senses(self):
//        self.add_sense("see-player", self.see_player)
//        self.add_sense("see-item", self.see_item)
//        self.add_sense("close-to-player", self.close_to_player)
//        self.add_sense("hit-object", self.was_hit)
//        self.add_sense("fail", lambda : 0)
//        self.add_sense("succeed", lambda : 1)
//        self.add_sense("is-rotating", self.is_rotating)
//        self.add_sense("is-walking", self.is_walking)
//        self.add_sense("is-stuck", self.is_stuck)

//#    def add_act(self, name, act):
//#        self.act_dict[name] = act
//#
//#    def add_sense(self, name, sense):
//#        self.sense_dict[name] = sense
//#
//#    def get_act(self, name):
//#        if self.act_dict.has_key(name):
//#            return self.act_dict[name]
//#        else:
//#            return None
//#        
//#    def get_sense(self, name):
//#        if self.sense_dict.has_key(name):
//#            return self.sense_dict[name]
//#        else:
//#            return None


//    def add_act(self, name, act):
//        self.agent.act_dict[name] = act

//    def add_sense(self, name, sense):
//        self.agent.sense_dict[name] = sense

//    def get_act(self, name):
//        if self.agent.act_dict.has_key(name):
//            return self.agent.act_dict[name]
//        else:
//            return None
        
//    def get_sense(self, name):
//        if self.agent.sense_dict.has_key(name):
//            return self.agent.sense_dict[name]
//        else:
//            return None

//    def bind_bot(self, bot):
//        self.bot = bot
//        bot.agent = self.agent
        
//    def see_player(self):
//        # print "See a player?"
//        # If the bot see any player, then return 1
//        if len(self.bot.view_players) > 0:
//            return 1
//        else:
//            return 0

//    def move_player(self):
//        #print "Move to player..."
//        # Find the first player and move to it
//        players = self.bot.view_players.values()
        
//        if len(players) > 0:
//            id = players[0]["Id"]
//        #    print str(id)
//            self.bot.send_message("RUNTO", {"Target" : str(id)})
            
//        return 1
        

//    def close_to_player(self):
//        #print "Checking to see if close to player..."
//        # If we are really close to the first player
//        closeness = 50
//        players = self.bot.view_players.values()
        
//        if len(players) > 0:
//            id = players[0]["Id"]
//            loc = players[0]["Location"]
//            (px, py, pz) = re.split(',', loc)
//            (sx, sy, sz) = re.split(',', self.bot.botinfo["Location"])
//            dis = find_distance((float(px),float(py)), (float(sx),float(sy)))
//            # print dis
//            if dis < closeness:
//                return 1
//            else:
//                return 0
//        else:
//            return 0
        

//    def see_item(self):
//        #print "See an item?"
//        # If we see an item, then return 1
//        if len(self.bot.view_items) > 0:
//            return 1
//        else:
//            return 0

//    def pickup_item(self):
//        #print "Pickup item..."
//        # Pickup the first item on the list
//        items = self.bot.view_items.values()
        
//        if len(items) > 0:
//            id = items[0]["Id"]           
//            self.bot.send_message("RUNTO", {"Target": str(id)})
//        return 1

//    def rotate(self, angle = None):
//        #print "Rotating..."
//        def turnleft():
//            self.bot.turn(90)
            
//        def turnright():
//            self.bot.turn(-90)
            
//        actions = (turnleft, turnright)
//        if angle == None:
//            random.choice(actions)() # Run the action randomly
//        else:
//            self.bot.move(angle)
//        return 1

//    def walk(self):
//        #print "Walking..."
//        return self.bot.move()

//    def stop_bot(self):
//        #print "Stopping Bot"
//        self.bot.send_message("STOP", {})
//        return 1

//    def was_hit(self):
//        #print "Was I hit?"
//        if self.bot.was_hit():
//            #print "yes!!!"
//            return 1
//        else:
//            #print "Ha, got you"
//            return 0

//    def is_rotating(self):
//        return self.bot.turning()

//    def is_walking(self):
//        return self.bot.moving()

//    def is_stuck(self):
//        return self.bot.stuck()



//#    def see_cookie(self):
//#        dirlist = os.listdir(os.getcwd())
//#        for x in dirlist:
//#            if x == "cookie":
//#                print "Fount cookie at " + os.getcwd()
//#                return 1
//#        print "Cookie Not Found at " + os.getcwd()
//#        return 0

//#    def change_dir(self):
//#        tmplist = os.listdir(os.getcwd())
//#        dirlist = []

//#        for x in tmplist:
//#            if os.path.isdir(x):
//#                dirlist.append(x)
//#        if os.getcwd() != "/home/andy/pyposh":
//#            dirlist.append("..")

//#        os.chdir(dirlist[random.randrange(len(dirlist))])
//#        print "Looking for cookie in " + os.getcwd()
//#        return 1
