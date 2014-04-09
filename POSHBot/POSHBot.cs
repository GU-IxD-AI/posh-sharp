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
using Posh_sharp.POSHBot.util;

namespace Posh_sharp.POSHBot
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

     
     //POSHBot created as a means of evaluating Behaviour Oriented Design [BOD]
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
    public class POSHBot : UTBehaviour
    {
        Regex firstIntMatcher;
        Regex middleIntMatcher;
        Regex spaceMatcher;
        Regex itemMatcher;
        Regex attributeMatcher; 

        IPAddress ip;
        int port;
        string botName;

        int team;
        /// <summary>
        /// things like hitting a wall
        /// </summary>
        List<Tuple<long,string>> events;
        Dictionary<string,string> conninfo;

        StreamWriter writer;

        protected internal Dictionary<string, string> gameinfo { get; private set; }
        protected internal Dictionary<string, UTPlayer> viewPlayers { get; private set; }
        protected internal List<InvItem> viewItems { get; private set; }
        protected internal Dictionary<string, NavPoint> navPoints { get; private set; }
        protected internal Dictionary<string, string> info { get; private set; }
		protected internal bool killConnection { get; private set;}

        Dictionary<string,string> sGameinfo;
        Dictionary<string,UTPlayer> sViewPlayers;
        List<InvItem> sViewItems;
        Dictionary<string,NavPoint> sNavPoints;
        Dictionary<string,string> sBotinfo;

        /// <summary>
        /// used to identify the last synchronized NavPoint in ConnectThread()
        /// </summary>
        private string sNavID;

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
        long hitTimestamp;
        bool threadActive;
        List<int> rotationHist;
        List<float> velocityHist;
        bool connReady;
        bool connectedToGame;
        Thread connThread;


        public POSHBot(AgentBase agent) : this(agent,null)
        {

        }

        public POSHBot(AgentBase agent, Dictionary<string,object> attributes)
            : base(agent, new string[] {}, new string[] {})
        {
            middleIntMatcher = new Regex(",(.*?),");
            firstIntMatcher = new Regex("(.*?),");
            spaceMatcher = new Regex(@"^(.+?)\s+(.+?)$");
            itemMatcher = new Regex(@"\{(.*?)\}");
            attributeMatcher = new Regex(@"\{(.*?)\s+(.*?)\}");
            // default connection values, use attributes to override
            ip = IPAddress.Parse("127.0.0.1");
            port = 3000;
            botName = "POSHbot";
            
            // Valid values for team are 0 and 1. If an invalid value is used gamebots
            //  will alternate the team on which each new bot is placed.
            team = -1;
        
            // all the rest is standard
            events  = new List<Tuple<long,string>>();
            conninfo = null;
            gameinfo = new Dictionary<string,string>();
            viewPlayers = new Dictionary<string,UTPlayer>(); 
            viewItems = new List<InvItem>();
            navPoints = new Dictionary<string,NavPoint>();
            info = new Dictionary<string,string>();
            
            sGameinfo = new Dictionary<string,string>();
            sViewPlayers = new Dictionary<string,UTPlayer>();
            sViewItems = new List<InvItem>();
            sNavPoints = new Dictionary<string,NavPoint>();
            sBotinfo = new Dictionary<string,string>();

            msgLog = new List<Tuple<string,Dictionary<string,string>>>();
            msgLogMax = 4096;
            sentMsgLog = new List<Tuple<string,Dictionary<string,string>>>();
            sentMsgLogMax = 6;
            hitTimestamp = -1;
            threadActive = false;
            killConnection = false;
            rotationHist = new List<int>();
            velocityHist = new List<float>();
            
            connReady = false;

            connThread = null;
        }


        private void ResetAttributes()
        {
            try
            {
                if (this.attributes.ContainsKey("botname"))
                    this.botName = (string)this.attributes["botname"];
                if (this.attributes.ContainsKey("team"))
                    this.team = (int)this.attributes["team"];
                if (this.attributes.ContainsKey("ip"))
                    this.ip = IPAddress.Parse((string)this.attributes["ip"]);
                if (this.attributes.ContainsKey("port"))
                    this.port = (int)this.attributes["port"];
            }
            catch (Exception e)
            { 
                Console.Out.WriteLine("additional parameters from init file could not be applied");
                if (_debug_)
                    Console.Out.WriteLine(e);
            }
        
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
                // TODO: there is no call for disconnect(); is this correct?
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
                ResetAttributes();
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
                connThread = new Thread(this.ConnectThread);
                connThread.Start();
                return true;
            }
            log.Error("Attempting to Connect() when thread already active");
            return false;
        }

        private Tuple<string,Dictionary<string,string>> ProcessItem(string item)
        {
            
            Dictionary<string,string> varDict = new Dictionary<string,string>();

            GroupCollection elements= spaceMatcher.Match(item).Groups;
            
            if (elements.Count <= 1)
                return new Tuple<string, Dictionary<string, string>>(item, varDict);

            string cmd = elements[1].Value;
            string varString = elements[2].Value;

            MatchCollection vars = itemMatcher.Matches(varString);
            foreach(Match var in vars)
            {
                GroupCollection pair = attributeMatcher.Match(var.Value).Groups;
                string attribute = pair[1].Value;
                string value = pair[2].Value;
                varDict[attribute] = value;
            }
            if (cmd == "DAM" || cmd == "PRJ")
                varDict["TimeStamp"] = POSH_sharp.sys.strict.TimerBase.CurrentTimeStamp().ToString();

            return new Tuple<string,Dictionary<string,string>>(cmd,varDict);
        }

        /// <summary>
        /// checks the bot's previous sent message against the provided one, returning true if they match
        /// </summary>
        /// <param name="?"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool IsPreviousMessage(string cmd,Dictionary<string,string> message)
        {
            if (sentMsgLog.Count == 0 || sentMsgLog[sentMsgLog.Count-1] != (new Tuple<string,Dictionary<string,string>>(cmd,message)) )
                return false;
            return true;
        }
        public void SendIfNotPreviousMessage(string cmd ,Dictionary<string,string> message)
        {
            if (!IsPreviousMessage(cmd,message))
                SendMessage(cmd,message);
        }
        
        public bool SendMessage(string command, Dictionary<string,string> dictionary)
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

        private string ReadDataInput(StreamReader reader)
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
            StreamReader reader = null;
            TcpClient client = null;
            writer = null;
            killConnection = false;
            
            try
            {
                client = new TcpClient(this.ip.ToString(), this.port);
                // client.Connect(ipe);
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
                return;
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

            // This loop waits for the initialisation messages
            while (!killConnection && !this.connectedToGame)
            {
                string dataIn = ReadDataInput(reader);
                if (dataIn == string.Empty)
                {
                    log.Error("Connection Closed from Remote End");
                    killConnection = true;
                    break;
                }
                // print dataIn
                Tuple<string,Dictionary<string,string>> result = ProcessItem(dataIn);

                switch (result.First)
                {
				case "HELLO_BOT":
					    SendMessage("READY",new Dictionary<string,string>());
                        break;
                    case "SNAV":
                        sNavPoints = new Dictionary<string,NavPoint>();
                        sNavID = "";
                        break;
                    case "NAV":
                        sNavPoints[result.Second["Id"]]=NavPoint.ConvertToNavPoint(result.Second);
                        sNavID = result.Second["Id"];
                        break;
                    case "SNPG":
                        sNavPoints[sNavID].SetNeighbors();
                        break;
                    case "INGP":
                        sNavPoints[sNavID].NGP.Add(new NavPoint.Neighbor(result.Second));
                        break;
                    case "ENGP":
                        sNavID = "";
                        break;
                    case "ENAV":
                        navPoints = sNavPoints;
                        GetNavigator().SetNavPoints(navPoints);
                        SendMessage("INIT",new Dictionary<string,string> {{"Name",this.botName},{"Team",this.team.ToString()}});
                        this.connectedToGame = true;
                        break;
                }
            }

            // Main Loop
            // Not everything is implemented. Just some basics
            while (!killConnection)
            {
                string dataIn = ReadDataInput(reader);
                if (dataIn == string.Empty)
                {
                    log.Error("Connection Closed from Remote End");
                    killConnection = true;
                    break;
                }
                //print "R>> " +  str(self) + x
                Tuple<string,Dictionary<string,string>> result = ProcessItem(dataIn);
                string [] syncStates = {"SLF","GAM","PLR","MOV","DOM","FLG","INV"};
                string [] pathStates = {"SPTH","IPTH","EPTH"};
                string [] events = {"WAL", "BMP"};
                
                this.msgLog.Add(result);

                if ( result.First =="BEG" )
                {
                    // When a sync batch is arriving, make sure the shadow
                    // states are cleared
                    this.sGameinfo = new Dictionary<string,string>();
                    this.sViewPlayers = new Dictionary<string,UTPlayer>();
                    this.sViewItems = new List<InvItem>();
                    this.sNavPoints = new Dictionary<string,NavPoint>();
                    this.sBotinfo = new Dictionary<string,string>();
                }
                else if ( syncStates.Contains(result.First) )
                    // These are sync. messages, handle them with another method
                    ProcessSync(result);
                else if (result.First == "END")
                {
                    SynShadowStates();
                    this.viewPlayers = this.sViewPlayers;

                }
                if (this.connectedToGame)
                {
                    this.connReady = true;
                    this.connectedToGame = false;
                }
                else if (events.Contains(result.First))
                    // The bot hit a wall or an actor, make a note
                    // of it in the events list with timestamp
                    this.events.Add(new Tuple<long, string>(TimerBase.CurrentTimeStamp(), result.ToString()));
                else if (result.First == "SEE")
                    // Update the player Position
                    
                    this.sViewPlayers[result.Second["Id"]] = new UTPlayer(result.Second);
                else if (pathStates.Contains(result.First))
                {
                    foreach (Behaviour behave in agent.getBehaviours())
                        if (behave is UTBehaviour)
                            ((UTBehaviour)behave).ReceivePathDetails(result.Second);
                    // pass the details to the movement behaviour
                    // ((Movement)agent.getBehaviour("Movement")).ReceivePathDetails(result.Second);
                }
                else if (result.First == "RCH")
                {
                    foreach (Behaviour behave in agent.getBehaviours())
                        if (behave is UTBehaviour)
                            ((UTBehaviour)behave).ReceiveCheckReachDetails(result.Second);
                    //((Navigator)agent.getBehaviour("Navigator")).ReceiveCheckReachDetails(result.Second);
                }
                else if (result.First == "PRJ")
                {
                    // incoming projectile
                    foreach (Behaviour behave in agent.getBehaviours())
                        if (behave is UTBehaviour)
                            ((UTBehaviour)behave).ReceiveProjectileDetails(result.Second);
                    //GetCombat().ReceiveProjectileDetails(result.Second);
                }
                else if (result.First == "DAM")
                {
                    foreach (Behaviour behave in agent.getBehaviours())
                        if (behave is UTBehaviour)
                            ((UTBehaviour)behave).ReceiveDamageDetails(result.Second);
                    // incoming projectile
                    //GetCombat().ReceiveDamageDetails(result.Second);
                }
                else if (result.First == "KIL")
                {
                    foreach (Behaviour behave in agent.getBehaviours())
                        if (behave is UTBehaviour)
                            ((UTBehaviour)behave).ReceiveKillDetails(result.Second);
                    // incoming projectile
                    // GetCombat().ReceiveKillDetails(result.Second);
                }
                else if (result.First == "DIE")
                {
                    foreach (Behaviour behave in agent.getBehaviours())
                        if (behave is UTBehaviour)
                            ((UTBehaviour)behave).ReceiveDeathDetails(result.Second);
                    // incoming projectile
                    // GetCombat().ReceiveDeathDetails(result.Second);
                }
            }

            log.Info("Closing Connection and Cleaning Up...");
            try
            {
                writer.Flush();
                writer.Close();
                reader.Close();
                if (client is TcpClient && client.Connected)
                    client.GetStream().Close();
                    client.Close();
                
            }
            catch (IOException)
            {
                log.Error("Could not close Reader or Writer on Socket.");
                // Skip the read loop
            }
            catch (Exception)
            {
                log.Error("Closing Connection to server failed.");
                // Skip the read loop
                killConnection = true;
            }

            this.threadActive = false;
            this.connReady = false;
            this.connThread = null;
            log.Info("Connection Thread Terminating...");
        }

        private void SynShadowStates()
        {
            // When a sync batch ends, we want to make the shadow
            // states that we were writing to to be the real one
            this.gameinfo = this.sGameinfo;
            this.viewPlayers = this.sViewPlayers;
            this.viewItems = this.sViewItems;
            this.info = this.sBotinfo;

            // Also a good time to trim the events list
            // Only keep the last 50 events
            if (this.events.Count > 50)
                this.events.RemoveRange(0, this.events.Count - 50);
            if (this.msgLog.Count > 1000)
                this.msgLog.RemoveRange(0, this.msgLog.Count - 1000);
        }

        /// <summary>
        /// all info from within the game are converted here 
        /// handles synchronisation messages
        /// </summary>
        /// <param name="message">Tuples[command,valuesDictionary]</param>
        internal void ProcessSync(Tuple<string,Dictionary<string,string>> message)
        {
            message.Second["TimeStamp"] = TimerBase.CurrentTimeStamp().ToString();
            switch (message.First){
                case "SLF":
                    // info about bot's state
                    this.sBotinfo = message.Second;
                    // Keep track of orientation so we can tell when we are moving
                    // Yeah, we only need to know the Yaw
                    this.rotationHist.Add(
                        int.Parse(
                        middleIntMatcher.Match( message.Second["Rotation"] )
                            .Groups[1].Value)
                    );
                    // Trim list to 3 entries
                    if ( this.rotationHist.Count > 3 )
                        this.rotationHist.RemoveRange(0,this.rotationHist.Count - 3);
                    // Keep track of velocity so we know when we are stuck
                    this.velocityHist.Add(
                        CalculateVelocity(message.Second["Velocity"])
                    );
                    // Trim it to 20 entries
                    if (this.velocityHist.Count > 20)
                        this.velocityHist.RemoveRange(0,velocityHist.Count - 20);
                    break;
                case "GAM":
                    this.sGameinfo = message.Second;
                    break;
                case "PLR":
                    // another character visible
                    // TODO: Test that this python statement is still true: For some reason, this doesn't work in ut2003
                    this.sViewPlayers[message.Second["Id"]] = new UTPlayer(message.Second);
                    break;
                case "NAV":
                    // a path marker
                    // TODO: Neither does this
                    // print "We have details about a nav point at " + values["Location"]
                    this.sNavPoints[message.Second["Id"]] = NavPoint.ConvertToNavPoint(message.Second);
                    break;
                case "INV":
                    // an object on the ground that can be picked up
                    this.sViewItems.Add(new InvItem(message.Second));
                    break;
                case "FLG":
                    // pass these details to the movement behaviour as that stores details of locations etc and may need them
                    // TODO: This needs some clean up as we just spread information which does not belong in all Behaviours
                    if (this._debug_)
                        foreach (KeyValuePair<string,string> elem in message.Second) 
                            Console.WriteLine(string.Format("{0} = {1}",elem.Key,elem.Value));
                    ((Movement)agent.getBehaviour("Movement")).ReceiveFlagDetails(message.Second);
                    // inform the combat behaviour as well
                    GetCombat().ReceiveFlagDetails(message.Second);
                    break; 
                default:
                    break;
            }
        }



        public static float CalculateVelocity(string velocityString)
        {
 	        Vector3 velocity = Vector3.ConvertToVector3(velocityString);

            return velocity.Distance2DFrom(Vector3.NullVector(),Vector3.Orientation.XY);
        }

        internal void Turn(int degrees)
        {
            int utAngle =(int) ((degrees * 65535) / 360.0);
            SendMessage("ROTATE",new Dictionary<string,string> { {"Amount",utAngle.ToString()} });
            // self.send_message("TURNTO", {"Pitch" : str(0)})
        }

        internal int GetYaw()
        {
            if (this.info.ContainsKey("Rotation"))
                return int.Parse(
                        middleIntMatcher.Match( this.info["Rotation"] )
                            .NextMatch()
                            .Value);
            return 0;
        }

        internal int GetPitch()
        {
            if (this.info.ContainsKey("Rotation"))
                return int.Parse(
                        firstIntMatcher.Match( this.info["Rotation"] )
                            .NextMatch()
                            .Value);
            return 0;
        }

        internal bool Inch()
        {
            Thread.Sleep(100);
            SendMessage("ACT", new Dictionary<string, string>() { {"Name", "Idle_Character02"} });
            return true;
        }

		internal bool Turning()
		{
			return Turning (386);
		}
        /// <summary>
        /// compares the most recent to the least recent rotationHist
        /// entry. If there is a descrepancy beyond the error fudge,
        /// then we say we are rotating
        /// </summary>
        /// <param name="fudge">standard 386: in UT units roughly 2 degrees</param>
        /// <returns></returns>
        internal bool Turning(int fudge)
        {
            if (this.rotationHist.Count > 0)
                if (Math.Abs( rotationHist[0] - rotationHist[rotationHist.Count-1] ) > fudge )
                    return true;

            return false;
        }
        
        /// <summary>
        /// if there is recent velocity return true
        /// </summary>
        /// <returns></returns>
        internal bool Moving()
        {
            if (velocityHist.Count > 0 && velocityHist[0] > 0)
                return true;

            return false;
        }

		internal bool Stuck()
		{
			return Stuck (0.0f);
		}
        /// <summary>
        /// if there is a period of not moving return true
        /// </summary>
        /// <param name="fudge">standard 0.0f</param>
        /// <returns></returns>
        internal bool Stuck(float fudge)
        {
            foreach (float v in this.velocityHist)
                if (v > fudge)
                    return false;

            return true;
        }

		internal bool WasHit()
		{
			return WasHit (2, 0);
		}
        /// <summary>
        /// Was the bot hit in the last lsec seconds
        /// </summary>
        /// <param name="lsec">how many secs look back</param>
        /// <param name="isec">number of seconds to inhibit WasHit</param>
        /// <returns></returns>
        internal bool WasHit(int lsec, int isec)
        {
            int lastEvents=0;
            long now = TimerBase.CurrentTimeStamp();

            foreach (Tuple<long,string> elem in this.events)
                if (elem.First > now - lsec)
                    lastEvents++;
            
            if (this.hitTimestamp < now - isec)
                {
                // Update the last Hit timestamp
                this.hitTimestamp = now;
                if (lastEvents > 0)
                    return true;
            }

            return false;
        }
    }
}




