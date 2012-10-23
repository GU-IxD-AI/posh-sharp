using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.test.examples.poshbot
{
    class BotAgent
    {
    }
}
//# Keeps a local copy of the bot state. Gamebots does not support
//# queries on the agent sense, it sends a copy of the environment
//# to the agent periodically.
//class Bot_Agent(Base):
//    def __init__(self, agent, ip, port, botname):
//        Base.__init__(self, agent)
//        self.ip = ip
//        self.port = port
//        self.botname = botname
//        self.events = []
//        self.conninfo = {}
//        self.gameinfo = {}
//        self.view_players = {}
//        self.view_items = {}
//        self.nav_points = {}
//        self.botinfo = {}
//        self.s_gameinfo = {}
//        self.s_view_players = {}
//        self.s_view_items = {}
//        self.s_nav_points = {}
//        self.s_botinfo = {}
//        self.msg_log = [] # Temp Log
//        self.msg_log_max = 4096 # Max Temp Log size
//        self.hit_timestamp = 0 # Used to inhibit was_hit()
//        self.thread_active = 0
//        self.kill_connection = 0
//        self.rotation_hist = []
//        self.velocity_hist = []
//        self.thread_active = 0
//        self.conn_ready = 0
//        self.conn_thread_id = None
                             

//    def proc_item(self, string):
//        (cmd, varstring) = re.compile('\s+').split(string, 1)
//        vars = re.compile('\{(.*?)\}').findall(varstring)
//        var_dict = {}
//        for var in vars:
//            (attr, value) = re.compile('\s+').split(var, 1)
//            var_dict[attr] = value
//        return (cmd, var_dict)

//    # Calls connect_thread in a new thread
//    def connect(self):
//        self.log.info("Connecting to Server")
//        if not self.conn_thread_id:
//            self.thread_active = 1
//            self.conn_thread_id = thread.start_new_thread(self.connect_thread, ())
//            return 1
//        else:
//            self.log.error("Attempting to Connect() when thread already active")
//            return 0


//    # This method runs inside a thread updating the agent state
//    # by reading from the network socket
//    def connect_thread(self):
//        self.kill_connection = 0
//        try:
//            self.sockobj = socket(AF_INET, SOCK_STREAM)
//            self.sockobj.connect((self.ip, int(self.port)))
//            self.sockin = self.sockobj.makefile('r')
//            self.sockout = self.sockobj.makefile('w')
//        except:
//            self.log.error("Connection to server failed")
//            self.kill_connection = 1 # Skip the read loops
//        else:
//            self.log.info("Connected to server")
//            self.kill_connection = 0
        
//        # This loop waits for the first NFO message
//        while not self.kill_connection:
//            try:
//                x = self.sockin.readline()
//            except:
//                self.log.error("Connection Error on readline()")
//                self.kill_connection = 1
//                break
                
//            if not x:
//                self.log.error("Connection Closed from Remote End")
//                self.kill_connection = 1
//                break
            
//            #print x
//            (cmd, dict) = self.proc_item(x)
//            if cmd == "NFO":
//                # Send INIT message
//                self.conninfo = dict
//                self.send_message("INIT", {"Name" : self.botname})
//                self.conn_ready = 1 # Ready to send messages
//                break

//        # Now the main loop
//        # Not everything is implemented. Just some basics
//        while not self.kill_connection:
//            try:
//                x = self.sockin.readline()
//            except:
//                self.log.error("Connection Error on readline()")
//                break
                
//            if not x:
//                self.log.error("Connection Closed from Remote End")
//                break
            
//            # print "R>> " + x
//            (cmd, dict) = self.proc_item(x)
//            sync_states = ("SLF","GAM","PLR","NAV","MOV","DOM","FLG","INV")
//            events = ("WAL", "BMP")
//            self.msg_log.append((cmd, dict))
//            if cmd == "BEG":
//                # When a sync batch is arriving, make sure the shadow
//                # states are cleared
//                self.s_gameinfo = {}
//                self.s_view_players = {}
//                self.s_view_items = {}
//                self.s_nav_points = {}
//                self.s_botinfo = {}
//            elif cmd in sync_states:
//                # These are sync. messages, handle them with another method
//                self.proc_sync(cmd, dict)
//            elif cmd == "END":
//                # When a sync batch ends, we cant to make the shadow
//                # states that we were writing to to be the real one
//                self.gameinfo = self.s_gameinfo
//                self.view_players = self.s_view_players
//                self.view_items = self.s_view_items
//                self.nav_points = self.s_nav_points
//                self.botinfo = self.s_botinfo
//                # Also a good time to trim the events list
//                # Only keep the last 50 events
//                self.events = self.events[-50:]
//                self.msg_log = self.msg_log[-1000:]
//            elif cmd in events:
//                # The bot hit a wall or an actor, make a note
//                # of it in the events list with timestamp
//                self.events.append((posh_utils.current_time(), cmd, dict))
//            elif cmd == "SEE":
//                # Update the player positions
//                self.view_players[dict["Id"]] = dict
//            else:
//                pass

//        self.log.info("Closing Sockets and Cleaning Up...")
//        try:
//            self.sockout.flush()
//            self.sockout.close()
//            self.sockin.close()
//            self.sockobj.close()
//        except:
//            self.log.error("Error closing files and sockets")
            
//        self.thread_active = 0
//        self.conn_ready = 0
//        self.conn_thread_id = None
//        self.log.info("Connection Thread Terminating...")

//    def disconnect(self):
//        self.kill_connection = 1

//    def send_message(self, cmd, dict):
//        string = cmd
//        for (attr, value) in dict.items():
//            string = string + " {" + attr + " " + value + "}"
//        string = string + "\r\n"
//        # print >> self.sockout, string
//        # print "S>> " + string
//        try:
//            self.sockout.write(string)
//            self.sockout.flush()
//        except:
//            self.log.error("Message : %s unable to send" % string)
//            return 0
//        else:
//            return 1

//    def proc_sync(self, command, values):
//        if command == "SLF":
//            self.s_botinfo = values
//            # Keep track of orientation so we can tell when we are moving
//            # Yeah, we only need to know the Yaw
//            self.rotation_hist.append(int(
//                re.search(',(.*?),', values['Rotation']).group(1)))
//            # Trim list to 3 entries
//            if len(self.rotation_hist) > 3:
//                del(self.rotation_hist[0])
//            # Keep track of velocity so we know when we are stuck
//            self.velocity_hist.append(self.calculate_velocity( \
//                values['Velocity']))
//            # Trim it to 20 entries
//            if len(self.velocity_hist) > 20:
//                del(self.velocity_hist[0])
            
//        elif command == "GAM":
//            self.s_gameinfo = values
//        elif command == "PLR":
//            # For some reason, this doesn't work in ut2003
//            self.s_view_players[values["Id"]] = values
//        elif command == "NAV":
//            # Neither does this
//            self.s_nav_points[values["Id"]] = values
//        elif command == "INV":
//            self.s_view_items[values["Id"]] = values
//        else:
//            pass

//    def turn(self, degrees):
//        utangle = int((degrees * 65535) / 360.0)
//        self.send_message("ROTATE", {"Amount" : str(utangle)})
//        # self.send_message("TURNTO", {"Pitch" : str(0)})
        
//    def get_yaw(self):
//        if self.botinfo.has_key("Rotation"):
//            return int(re.search(',(.*?),', self.botinfo["Rotation"]).group(1))
//        else:
//            return None
        
//    def get_pitch(self):
//        if self.botinfo.has_key("Rotation"):
//            return int(re.match('(.*?),', self.botinfo["Rotation"]).group(1))
//        else:
//            return None
        
//    def move(self):
//        self.send_message("INCH", {})
//        return 1

//    # Was the bot hit in the last 2 seconds
//    def was_hit(self):
//        lsec = 2 # How many seconds to look back to
//        isec = 0 # Number of seconds to inhibit consecutive was_hits
//        now = posh_utils.current_time()
//        def timefilter(item):
//            (timestamp, command, value) = item
//            if timestamp > now - lsec:
//                return 1
//            else:
//                return 0
        
//        # Filter the events from the last lsec seconds
//        lastevents = filter(timefilter, self.events)

//        if self.hit_timestamp > now - isec:
//            # Update the last hit timestamp
//            return 0
//        else:
//            # Update the last hit timestamp
//            self.hit_timestamp = now
//            if len(lastevents) > 0:
//                return 1
//            else:
//                return 0

//    def turning(self):
//        # compares the most recent to the leat recent rotation_hist
//        # entry. If there is a descrepancy beyond the error fudge,
//        # then we say we are rotating
//        fudge = 386 # in UT units, roughly 2 degrees
//        if len(self.rotation_hist) > 0:
//            c_rot = self.rotation_hist[0]
//            e_rot = self.rotation_hist[-1]
//            diff = abs(c_rot - e_rot)
//            if diff > fudge:
//                return 1
            
//        return 0
        
//    def moving(self):
//        # If there is recent velocity, return 1
//        if len(self.velocity_hist) > 0:
//            if self.velocity_hist[0] > 0:
//                return 1
//        return 0

//    def stuck(self):
//        # If there is a period of no movement, then return 1
//        fudge = 0
//        for v in self.velocity_hist:
//            if v > fudge:
//                return 0
//        return 1
        
//    def calculate_velocity(self, v):
//        (vx, vy, vz) = re.split(',', v)
//        vx = float(vx)
//        vy = float(vy)
//        return find_distance((0,0), (vx, vy))
