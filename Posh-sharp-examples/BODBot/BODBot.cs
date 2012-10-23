using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using POSH_sharp.sys.strict;
using Posh_sharp_examples.BODBot.util;

namespace Posh_sharp_examples.BODBot
{
//#  We need to start a comms thread in order to get updates
//#  to the agent status from the server.
//from socket import *
//from POSH import Behaviour
//from POSH.utils import current_time
//import re #re is for Regular Expressions
//import thread
//import sys
//import time

     
     //BODbot created as a means of evaluating Behaviour Oriented Design [BOD]
     //Much code here re-used from Andy Kwong's poshbot
     //It has been refactored on the 29/08/07 to make Bot a behaviour and clean
     //up the behaviour structure a bit.
     
    
    /// <summary>
    /// The Bot behaviour.
    ///    
    /// This behaviour does not provide any actions that are directly used in plans.
    /// Rather, it establishes the connection with UT and provides methods to
    /// control the bot which can be used by other behaviours.
    /// 
    /// The behaviour keeps a local copy of the bot state. Gamebots do not support
    /// queries on the agent sense, it sends a copy of the environment to the
    /// agent periodically.
    /// 
    /// To change connection IP, port and the bot's name, use the attributes
    /// Bot.ip, Bot.port and Bot.botname.
    /// </summary>
    public class BODBot : Behaviour
    {
        //import utilityfns

        //# import behaviour classes
        //import movement
        //import combat

        IPAddress ip;
        int port;
        private NetworkStream _stream;
        string botName;

        int team;
        /// <summary>
        /// things like hitting a wall
        /// </summary>
        List<string> events;
        Dictionary<string,string> conninfo;

        StreamReader reader;
        StreamWriter writer;
        
        List<string> gameinfo;
        public Dictionary<string,UTPlayer> viewPlayers { protected internal get; private set;}
        List<string> viewItems;
        protected internal List<NavPoint> navPoints;
        public Dictionary<string,string> botinfo { protected internal get; private set;}

        List<string> sGameinfo;
        Dictionary<string,object> sViewPlayers;
        List<string> sViewItems;
        List<NavPoint> sNavPoints;
        Dictionary<string,string> sBotinfo;

        /// <summary>
        /// Temp Log for message received
        /// </summary>
        List<Tuple<string,Dictionary<string,string>>> msgLog;
        /// <summary>
        /// Max Temp Log size
        /// </summary>
        int msgLogMax;
        /// <summary>
        /// Temp Log for messages sent
        /// </summary>
        List<Tuple<string,Dictionary<string,string>>> sentMsgLog;
        /// <summary>
        /// Max Temp sent Log size
        /// </summary>
        int sentMsgLogMax;
        /// <summary>
        /// Used to inhibit was_hit()
        /// </summary>
        bool hitTimestamp;
        bool threadActive;
        bool killConnection;
        List<string> rotationHist;
        List<string> velocityHist;
        bool connReady;
        Thread connThread;




        public BODBot(AgentBase agent, Dictionary<string,object> attributes = null)
            : base(agent, new string[] {}, new string[] {},attributes)
        {
            // default connection values, use attributes to override
            ip = IPAddress.Parse("127.0.0.1");
            port = 3000;
            botName = "BODbot";
            
            // Valid values for team are 0 and 1. If an invalid value is used gamebots
            //  will alternate the team on which each new bot is placed.
            team = -1;
        
            // all the rest is standard
            events  = new List<string>();
            conninfo = null;
            gameinfo = new List<string>();
            viewPlayers = new Dictionary<string,UTPlayer>(); 
            viewItems = new List<string>();
            navPoints = new List<NavPoint>();
            botinfo = new Dictionary<string,string>();
            
            sGameinfo = new List<string>();
            sViewPlayers = new Dictionary<string,object>();
            sViewItems = new List<string>();
            sNavPoints = new List<NavPoint>();
            sBotinfo = new Dictionary<string,string>();

            msgLog = new List<Tuple<string,Dictionary<string,string>>>();
            msgLogMax = 4096;
            sentMsgLog = new List<Tuple<string,Dictionary<string,string>>>();
            sentMsgLogMax = 6;
            hitTimestamp = false;
            threadActive = false;
            killConnection = false;
            rotationHist = new List<string>();
            velocityHist = new List<string>();
            
            connReady = false;
            connThread = null;
        }

        /// <summary>
        /// Attempts connecting to the UT server.
        /// 
        /// If the bot is currently connected, it disconnects, waits a bit,
        /// and then reconnects.
        /// </summary>
        public override bool  Reset()
        {
            // disconnect
            if (threadActive)
            {
                // TODO: ther eis no call for disconnect(); is this correct?
                log.Debug("Currently connected, trying to disconnect");
                // wait for 3 seconds to disconnection
                int timeout = 0;
                while (threadActive && timeout++ < 30)
                {
                    Thread.Sleep(10);

                }
            }
            // connect only if not connected
            if (!threadActive)
            {
                Connect();
                return true;
            }
            else
            {
                log.Error("Reset failed, as failed to disconnect");
                return false;
            }
        }

        /// <summary>
        /// Returns if the behaviour is ready.
        /// 
        /// This method is called by agent.loopThread to make sure that the
        /// behaviour is OK in every cycle. This behaviour is OK if it is connected
        /// to UT. Otherwise it isn't.
        /// </summary>
        /// <returns></returns>
        public override bool  CheckError()
        {
 	         if (connReady)
                 return false;
             else 
                 return true;
        }

        /// <summary>
        /// Prepares the bot to exit by disconnecting from UT.
        /// </summary>
        public override void  ExitPrepare()
        {
 	         Disconnect();
        }

        public void Disconnect()
        {
            killConnection = true;
        }
        /// <summary>
        /// Calls connect_thread in a new thread
        /// </summary>
        public bool Connect()
        {
            log.Info(string.Format("Connecting to Server ({0}:{1})", ip, port));
            if(connThread == null)
            {
                threadActive = true;
                connThread = new Thread(new ThreadStart(ConnectThread));
                return true;
            }
            log.Error("Attempting to Connect() when thread already active");
            return false;
        }

        private Tuple<string,Dictionary<string,string>> ProcessItem(string item)
        {
            Regex spaceMatcher =  new Regex(@"\s+");
            Regex itemMatcher = new Regex(@"\{(.*?)\}");
            Dictionary<string,string> varDict = new Dictionary<string,string>();
            
            string cmd = spaceMatcher.Split(item,1)[0];
            string varString = spaceMatcher.Split(item,1)[1];

            Match vars = itemMatcher.Match(varString);
            foreach(Group var in vars.Groups)
            {
                string attribute = spaceMatcher.Split(var.Captures[0].Value,1)[0];
                string value = spaceMatcher.Split(var.Captures[0].Value,1)[1];
                varDict[attribute] = value;
            }
            if (cmd == "DAM" && cmd == "PRJ")
                varDict["timestamp"] = POSH_sharp.sys.strict.TimerBase.TimeStamp().ToString();

            return new Tuple<string,Dictionary<string,string>>(cmd,varDict);
        }

        public bool sendMessage(string command, Dictionary<string,string> dictionary)
        {
            string output = command;
            this.sentMsgLog.Add(new Tuple<string,Dictionary<string,string>>(command,dictionary));
            
            // does the list need truncating?
            while (sentMsgLog.Count > sentMsgLogMax)
                sentMsgLog.RemoveAt(0);
            
            foreach (KeyValuePair<string,string> item in dictionary)
                // OLD-COMMENT: Only works when using str() otherwise error because target is a tuple
                // not all targets are tuples, find out why. FA
                output += " {"+item.Key +" "+ item.Value+ "}";
            // print "About to send " + string
            output += "\r\n";

            try
            {
                writer.Write(output);
                writer.Flush();
            }
            catch (Exception)
            {
                log.Error(string.Format("Message : {0} unable to send",output));
                return false;
            }

            return true;
        }

        private string ReadDataInput()
        {
            string dataIn=string.Empty;
            try 
                {
                    dataIn = reader.ReadLine();
                }
                catch (Exception)
                {
                    log.Error("Connection Error on readline()");
                    killConnection = true;
                }
            return dataIn;
        }

        /// <summary>
        /// This method runs inside a thread updating the agent state
        /// by reading from the network socket
        /// </summary>
        void ConnectThread()
        {
            NetworkStream stream = null;
            reader = null;
            writer = null;
            killConnection = false;
            
            try
            {
                IPEndPoint ipe = new IPEndPoint(this.ip, this.port);
                TcpClient client = new TcpClient(ipe);
                client.Connect(ipe);

                if(client.Connected)
                {
                    stream = client.GetStream();
                    
                }
            }
            catch (Exception)
            {
                log.Error("Connection to server failed");
                // Skip the read loop
                killConnection = true;
            }

            try
            {
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
            }
            catch (IOException)
            {
                log.Error("Could not establish Reader or Writer on Socket.");
                // Skip the read loop
                killConnection = true;
            }
            if (reader is StreamReader)
            {
                // TODO: why does it write to error? shouldnt it be Info
                log.Error("Connected to server");
                killConnection = false;
            }

            // This loop waits for the first NFO message
            while (!killConnection)
            {
                string dataIn = ReadDataInput();
                if (dataIn == string.Empty)
                {
                    log.Error("Connection Closed from Remote End");
                    killConnection = true;
                    break;
                }
                // print dataIn
                Tuple<string,Dictionary<string,string>> result = ProcessItem(dataIn);
                if (result.First == "NFO")
                {
                    // Send INIT message
                    this.conninfo = result.Second;
                    sendMessage("INIT", new Dictionary<string, string> {{"Name" , botName}, {"Team", team.ToString()}});
                    // ready to send messages
                    connReady = true;
                    break;
                
                }
            }

            // Main Loop
            // Not everything is implemented. Just some basics
            while (!killConnection)
            {
                string dataIn = ReadDataInput();
                if (dataIn == string.Empty)
                {
                    log.Error("Connection Closed from Remote End");
                    killConnection = true;
                    break;
                }
                //print "R>> " +  str(self) + x
                Tuple<string,Dictionary<string,string>> result = ProcessItem(dataIn);
                string [] syncStates = {"SLF","GAM","PLR","NAV","MOV","DOM","FLG","INV"};
                string []events = {"WAL", "BMP"};
                this.msgLog.Add(result);

                if ( result.First =="BEG" )
                {
                    // When a sync batch is arriving, make sure the shadow
                    // states are cleared
                    this.sGameinfo = new List<string>();
                    this.sViewPlayers = new Dictionary<string,object>();
                    this.sViewItems = new List<string>();
                    this.sNavPoints = new List<NavPoint>();
                    this.sBotinfo = new Dictionary<string,string>();
                }
                else if ( syncStates.Contains(result.First) )
                    // These are sync. messages, handle them with another method
                    ProcessSync(result);
                else if (result.First == "END")
                {
                    // When a sync batch ends, we want to make the shadow
                    // states that we were writing to to be the real one
                    this.gameinfo = this.sGameinfo;
                    this.viewPlayers = this.sViewPlayers;
                    this.viewItems = this.sViewItems;
                    this.navPoints = this.sNavPoints;
                    this.botinfo = this.sBotinfo;

                    // Also a good time to trim the events list
                    // Only keep the last 50 events
                    if (this.events.Count > 50)
                        this.events.RemoveRange(0, this.events.Count - 50);
                    if (this.msgLog.Count > 1000)
                        this.msgLog.RemoveRange(0, this.msgLog.Count - 1000);
                }
                else if ( events.Contains(result.First) )
                    // The bot hit a wall or an actor, make a note
                    // of it in the events list with timestamp
                    this.events.Add(TimerBase.TimeStamp()+" "+ result.ToString());
                else if (result.First == "SEE")
                    // Update the player Position
                    this.viewPlayers[result.Second["Id"]] = result.Second;
                else if (result.First == "PTH")
                    // pass the details to the movement behaviour
                    (Movement)agent.getBehaviour("Movement").receive_pth_details(dict)
                    else if (result.First == "SEE")
                        else if (result.First == "SEE")
            }
        }
    }


}
        while not self.kill_connection:
            if cmd == "BEG":

            elif cmd in sync_states:
                # These are sync. messages, handle them with another method
                self.proc_sync(cmd, dict)
            elif cmd == "END":

            elif cmd in events:
                # The bot hit a wall or an actor, make a note
                # of it in the events list with timestamp
                self.events.append((current_time(), cmd, dict))
            elif cmd == "SEE":
                # Update the player positions
                self.view_players[dict["Id"]] = dict
            elif cmd == "PTH":
                # pass the details to the movement behaviour
                self.agent.Movement.receive_pth_details(dict)
            elif cmd == "RCH":
                self.agent.Movement.receive_rch_details(dict)
            elif cmd == "PRJ": # incoming projectile
                self.agent.Combat.receive_prj_details(dict)
            elif cmd == "DAM": # damage taken
                self.agent.Combat.receive_dam_details(dict)
            elif cmd == "KIL": # some other player died
                self.agent.Combat.receive_kil_details(dict)
            elif cmd == "DIE": # this player died
                self.agent.Combat.receive_die_details(dict)
            else:
                pass

        self.log.info("Closing Sockets and Cleaning Up...")
        try:
            self.sockout.flush()
            self.sockout.close()
            self.sockin.close()
            self.sockobj.close()
        except:
            self.log.error("Error closing files and sockets")
            
        self.thread_active = 0
        self.conn_ready = 0
        self.conn_thread_id = None
        self.log.info("Connection Thread Terminating...")






    // all info from within the game are converted here
    #handles synchronisation messages
    def proc_sync(self, command, values):
        if command == "SLF": #info about bot's state
            self.s_botinfo = values
            # Keep track of orientation so we can tell when we are moving
            # Yeah, we only need to know the Yaw
            self.rotation_hist.append(int(
                re.search(',(.*?),', values['Rotation']).group(1)))
            # Trim list to 3 entries
            if len(self.rotation_hist) > 3:
                del(self.rotation_hist[0])
            # Keep track of velocity so we know when we are stuck
            self.velocity_hist.append(self.calculate_velocity( \
                values['Velocity']))
            # Trim it to 20 entries
            if len(self.velocity_hist) > 20:
                del(self.velocity_hist[0])
            
        elif command == "GAM": #info about the game
            self.s_gameinfo = values
        elif command == "PLR": #another character visible
            # For some reason, this doesn't work in ut2003
            self.s_view_players[values["Id"]] = values
        elif command == "NAV": #a path marker
            # Neither does this
            #print "We have details about a nav point at " + values["Location"]
            self.s_nav_points[values["Id"]] = values
        elif command == "INV": #an object on the ground that can be picked up
            #print values
            self.s_view_items[values["Id"]] = values
        elif command == "FLG": #info about a flag
            #pass these details to the movement behaviour as that stores details of locations etc and may need them
            values["timestamp"] = current_time()
            print "\n".join(["%s=%s" % (k, v) for k, v in values.items()])
            
            self.agent.Movement.receive_flag_details(values)
            # inform the combat behaviour as well
            self.agent.Combat.receive_flag_details(values)
            #print("We have details about a flag.  Its values is: " + values["State"]);
        else:
            pass
    
    def turn(self, degrees):
        utangle = int((degrees * 65535) / 360.0)
        self.send_message("ROTATE", {"Amount" : str(utangle)})
        # self.send_message("TURNTO", {"Pitch" : str(0)})
        
    def get_yaw(self):
        if self.botinfo.has_key("Rotation"):
            return int(re.search(',(.*?),', self.botinfo["Rotation"]).group(1))
        else:
            return None
        
    def get_pitch(self):
        if self.botinfo.has_key("Rotation"):
            return int(re.match('(.*?),', self.botinfo["Rotation"]).group(1))
        else:
            return None
        
    def move(self):
        self.send_message("INCH", {})
        return 1

    # Was the bot hit in the last 2 seconds
    def was_hit(self):
        lsec = 2 # How many seconds to look back to
        isec = 0 # Number of seconds to inhibit consecutive was_hits
        now = current_time()
        def timefilter(item):
            (timestamp, command, value) = item
            if timestamp > now - lsec:
                return 1
            else:
                return 0
        
        # Filter the events from the last lsec seconds
        lastevents = filter(timefilter, self.events)

        if self.hit_timestamp > now - isec:
            # Update the last hit timestamp
            return 0
        else:
            # Update the last hit timestamp
            self.hit_timestamp = now
            if len(lastevents) > 0:
                return 1
            else:
                return 0

    def turning(self):
        # compares the most recent to the leat recent rotation_hist
        # entry. If there is a descrepancy beyond the error fudge,
        # then we say we are rotating
        fudge = 386 # in UT units, roughly 2 degrees
        if len(self.rotation_hist) > 0:
            c_rot = self.rotation_hist[0]
            e_rot = self.rotation_hist[-1]
            diff = abs(c_rot - e_rot)
            if diff > fudge:
                return 1
            
        return 0
        
    def moving(self):
        # If there is recent velocity, return 1
        if len(self.velocity_hist) > 0:
            if self.velocity_hist[0] > 0:
                return 1
        return 0

    def stuck(self):
        # If there is a period of no movement, then return 1
        fudge = 0
        for v in self.velocity_hist:
            if v > fudge:
                return 0
        return 1
        
    def calculate_velocity(self, v):
        (vx, vy, vz) = re.split(',', v)
        vx = float(vx)
        vy = float(vy)
        return utilityfns.find_distance((0,0), (vx, vy))
