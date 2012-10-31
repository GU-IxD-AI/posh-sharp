using System;
using System.Collections.Generic;
using System.Text;
using POSH_sharp.sys;
using POSH_sharp.sys.annotations;
using Posh_sharp.examples.BODBot.util;

namespace Posh_sharp.examples.BODBot
{
    public class Movement : Behaviour
    {
        PositionsInfo posInfo;
        string pathHomeId;
        string reachPathHomeId;
        string pathToEnemyBaseId;
        string reachPathToEnemyBaseID;

        public Movement(AgentBase agent) 
            : base(agent, 
            new string[] {"walk_to_nav_point", "to_enemy_flag", 
                            "to_own_base", "to_own_flag", "to_enemy_base", "inch",
                            "runto_medical_kit", "runto_weapon"},
            new string[] {"AtEnemyBase", "AtOwnBase", "KnowEnemyBasePos",
                            "KnowOwnBasePos", "ReachableNavPoint",
                            "EnemyFlagReachable", "OurFlagReachable",
                            "SeeEnemy", "SeeReachableMedicalKit",
                            "SeeReachableWeapon", "TooCloseForPath"})
        {
            this.posInfo = new PositionsInfo();
            pathHomeId = "PathHome";
            reachPathHomeId = "ReachPathHome";
            pathToEnemyBaseId = "PathThere";
            reachPathToEnemyBaseID = "ReachPathThere";

        }

        private BODBot getBot(string name="Bot")
        {
            return ((BODBot)agent.getBehaviour("Bot"));
        }

        ///
        /// SENSES
        /// 
        
                
        private bool atTargetLocation(NavPoint target, int distanceTolerance)
        {
            if ( !getBot().botinfo.ContainsKey("Location") )
                return false;
            Vector3 location = Vector3.ConvertToVector3(getBot().botinfo["Location"]);

            if (target == null)
                return false;
            else
            {
                // this distance may need adjusting in future (we may also wish to consider the Z axis)
                if (location.DistanceFrom(target.Location) < distanceTolerance)
                    return true;
                else 
                    return false;
            }
        }

        /// <summary>
        /// returns 1 if we're near enough to enemy base
        /// </summary>
        [ExecutableSense("AtEnemyBase")]
        public bool AtEnemyBase()
        {
            return atTargetLocation(posInfo.enemyBasePos,100);
        }

        /// <summary>
        /// returns 1 if we're near enough to our own base
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("AtOwnBase")]
        public bool AtOwnBase()
        {
            return atTargetLocation(posInfo.ownBasePos,10);
        }

        /// <summary>
        /// returns 1 if we have a location for the enemy base
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("KnowEnemyBasePos")]
        public bool KnowEnemyBasePos()
        {
            return (posInfo.enemyBasePos != null) ? true : false; 
        }

        /// <summary>
        /// returns 1 if we have a location for our own base
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("KnowOwnBasePos")]
        public bool KnowOwnBasePos()
        {
            return (posInfo.ownBasePos != null) ? true : false; 
        }

        /// <summary>
        /// returns 1 if there's a reachable nav point in the bot's list which we're not already at
        /// </summary>
        /// <returns></returns>
        [ExecutableSense("ReachableNavPoint")]
        public bool ReachableNavPoint()
        {
            Vector3 location;
            NavPoint navPoint;
            int distanceTolerance = 30; // how near we must be to be thought of as at the nav point

            if ( getBot().botinfo.ContainsKey("Location") )
                location = Vector3.ConvertToVector3(getBot().botinfo["Location"]);
            else
                // if we don't know where we are, treat it as (0,0,0) 
                // as that will just mean we go to the nav point even if we're close by
                location = new Vector3();

            Console.Out.WriteLine(location.ToString());

            // is there already a navpoint we're aiming for?
            
            if (posInfo.chosenNavPoint != null)
            {
                navPoint = posInfo.chosenNavPoint;
                if ( navPoint.DistanceFrom(location) > distanceTolerance )
                    return true;
                else
                {
                    posInfo.visitedNavPoints.Add(navPoint.Id);
                    posInfo.chosenNavPoint = null;
                }
            }
            // now look at the list of navpoints the bot can see
            if ( getBot().navPoints == null || getBot().navPoints.Count() < 1 )
            {
                Console.Out.WriteLine("  no nav points");
                return false;
            }
            else
            {
                // nav_points is a list of tuples.  Each tuple contains an ID and a dictionary of attributes as defined in the API
                // Search for reachable nav points
                List<NavPoint> possibleNPs = new List<NavPoint>(GetReachableNavPoints(getBot().navPoints.ToArray(), distanceTolerance, location));

                // now work through this list of NavPoints until we find once that we haven't been to
                // or the one we've been to least often
                if (possibleNPs.Count == 0)
                {
                    Console.Out.WriteLine("    No PossibleNPs");
                    return false;
                }
                else
                {
                    posInfo.chosenNavPoint = GetLeastOftenVisitedNavPoint(possibleNPs);
                    Console.Out.WriteLine("    Possible NP, returning 1");
                    return true;
                }
            }
        }

        /// <summary>
        /// not well optimised because it also checks each duplicate
        /// </summary>
        /// <param name="navPoints"></param>
        /// <returns></returns>
        internal NavPoint GetLeastOftenVisitedNavPoint(List<NavPoint> navPoints)
        {

            // FIX: corrected mistake in the original implementation which should have faulted the code

            int currentMin = posInfo.visitedNavPoints.Count(g => g == navPoints[0].Id);
            NavPoint currentMinNP = navPoints[0];

            foreach (NavPoint point in navPoints)
            {
                int currentCount = posInfo.visitedNavPoints.Count(g => g == point.Id);
                if (currentCount < currentMin)
                {
                    currentMin = currentCount;
                    currentMinNP = point;
                }
            }

            return currentMinNP;
        }

        internal NavPoint [] GetReachableNavPoints(NavPoint [] navPoints, int distanceTolerance, Vector3 target)
        {
            List<NavPoint> result = new List<NavPoint>();

            foreach (NavPoint currentNP in navPoints )
                if  (currentNP.Reachable && currentNP.Distance2DFrom(target,Vector3.Orientation.XY) > distanceTolerance)
                    result.Add(currentNP);
            
            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>returns 1 if the enemy flag is specified as reachable</returns>
        [ExecutableSense("EnemyFlagReachable")]
        public bool EnemyFlagReachable()
        {
            if (this.posInfo.hasEnemyFlagInfoExpired())
                posInfo.expireEnemyFlagInfo();

            // debug
            if (_debug_)
            {
                Console.Out.WriteLine("in EnemyFlagReachable");
                if (posInfo.enemyFlagInfo.Count() > 0)
                    Console.Out.WriteLine(posInfo.enemyFlagInfo.ToString());
            }

            // Made simpler FA
            if (posInfo.enemyFlagInfo.Count() > 0 && bool.Parse(posInfo.enemyFlagInfo["Reachable"]) == true)
                return true;

            return false;
            
        }

                /// <summary>
        /// 
        /// </summary>
        /// <returns>returns 1 if our flag is specified as reachable</returns>
        [ExecutableSense("OurFlagReachable")]
        public bool OurFlagReachable()
        {
            if (this.posInfo.hasOurFlagInfoExpired())
                posInfo.expireOurFlagInfo();

            // debug
            if (_debug_)
            {
                Console.Out.WriteLine("in OurFlagReachable");
                if (posInfo.ourFlagInfo.Count() > 0)
                    Console.Out.WriteLine(posInfo.ourFlagInfo.ToString());
            }

            // Made simpler FA
            if (posInfo.ourFlagInfo.Count() > 0 && bool.Parse(posInfo.ourFlagInfo["Reachable"]) == true)
                return true;

            return false;
            
        }

        [ExecutableSense("SeeEnemy")]
        public bool SeeEnemy()
        {
            if (getBot().viewPlayers.Count  == 0 ||  getBot().botinfo.Count == 0)
                return false;
            
            // work through, looking for an enemy
            string ourTeam = getBot().botinfo["Team"];
            UTPlayer [] players = getBot().viewPlayers.Values.ToArray();
            foreach (UTPlayer currentPlayer in players )
                if ( currentPlayer.Team != ourTeam )
                {
                    if (_debug_)
                        Console.Out.WriteLine("SeeEnemy: we can see an enemy!");
                    return true;
                }
            
            return false;
        }

        [ExecutableSense("SeeReachableMedicalKit")]
        public bool SeeReachableMedicalKit()
        {
            if (getBot().viewItems.Count < 1)
                return false;

            // look through items for a Medkit
            foreach (INVItem item in getBot().viewItems)
            {

            }
        }
    }

    
}

        
    def see_reachable_medical_kit(self):
        if len(self.agent.Bot.view_items) < 1:
            return 0
        else:
            # look through for a medical kit
            ItemValues = self.agent.Bot.view_items.values()
            for CurrentItem in ItemValues:
                if (CurrentItem["Class"].find("Health") != -1 or CurrentItem["Class"].find("MedBox")) and CurrentItem["Reachable"] == "True":
                    return 1
            return 0
            
    def see_reachable_weapon(self):
        if len(self.agent.Bot.view_items) < 1:
            return 0
        else:
            # look through for a weapon
            ItemValues = self.agent.Bot.view_items.values()
            for CurrentItem in ItemValues:
                if utilityfns.is_known_weapon_class(CurrentItem["Class"]) and CurrentItem["Reachable"] == "True":
                    return 1
            return 0
            
    # see PositionsInfo class for comments on TooCloseForPath
    def too_close_for_path(self):        
        if self.PosInfo.has_too_close_for_path_expired():
            self.PosInfo.expire_too_close_for_path()
    
        if self.PosInfo.TooCloseForPath:
            print "we are too close for path"
        return self.PosInfo.TooCloseForPath
                
    # === ACTIONS ===
    
    def runto_medical_kit(self):
        if len(self.agent.Bot.view_items) < 1:
            return 1
        else:
            # look through for a medical kit
            ItemValues = self.agent.Bot.view_items.values()
            for CurrentItem in ItemValues:
                if (CurrentItem["Class"].find("Health") != -1 or CurrentItem["Class"].find("MedBox")) and CurrentItem["Reachable"] == "True":
                    self.send_runto_or_strafe_to_location(CurrentItem["Location"])
            return 1
            
    def runto_weapon(self):
        if len(self.agent.Bot.view_items) < 1:
            return 1
        else:
            # look through for a weapon
            ItemValues = self.agent.Bot.view_items.values()
            for CurrentItem in ItemValues:
                if utilityfns.is_known_weapon_class(CurrentItem["Class"]) and CurrentItem["Reachable"] == "True":
                    print "runto weapon",
                    print CurrentItem["Class"]
                    self.send_runto_or_strafe_to_location(CurrentItem["Location"])
                    return 1
            return 1
    
    # Runs to the ChosenNavPoint
    def walk_to_nav_point(self):
        #print "walk_to_nav_point: " + utilityfns.location_tuple_to_string(self.PosInfo.ChosenNavPoint)
        
        # have we already sent it?
        #if not utilityfns.is_previous_message(self.agent.Bot, ("RUNTO", {"Location" : utilityfns.location_tuple_to_string(self.PosInfo.ChosenNavPoint)})):
        #    self.agent.Bot.send_message("RUNTO", {"Location" : utilityfns.location_tuple_to_string(self.PosInfo.ChosenNavPoint)})
            #print "sending message"
        #else:
            #print "already sent"
            
        # new version just calls the utility function
        #utilityfns.send_if_not_prev(self.agent.Bot, ("RUNTO", {"Location" : utilityfns.location_tuple_to_string(self.PosInfo.ChosenNavPoint)}))
        
        #even newer version has a strafe check
        self.send_runto_or_strafe_to_location(utilityfns.location_tuple_to_string(self.PosInfo.ChosenNavPoint))
        
        return 1
            
    # runs to the enemy flag
    def to_enemy_flag(self):
        print "!!in to_enemy_flag"
        print "\n".join(["%s=%s" % (k, v) for k, v in self.PosInfo.EnemyFlagInfo.items()])
        
        if self.PosInfo.has_enemy_flag_info_expired():
            self.PosInfo.expire_enemy_flag_info()
        
        if self.PosInfo.EnemyFlagInfo != {}:
            self.agent.Bot.send_message("RUNTO", {"Target" : self.PosInfo.EnemyFlagInfo["Id"]})
        return 1
            
    def to_own_flag(self):  
        if self.PosInfo.has_our_flag_info_expired():
            self.PosInfo.expire_our_flag_info()
    
        if self.PosInfo.OurFlagInfo != {}:
            # was self.agent.Bot.send_message("RUNTO", {"Location" : self.PosInfo.OurFlagInfo["Location"]})
            self.send_runto_or_strafe_to_location(self.PosInfo.OurFlagInfo["Location"])
        return 1
            
    # runs to the bot's own base by getting a list of navpoints showing the way there
    def to_own_base(self):
        print "to_own_base"
        DistanceTolerance = 30
        # If we don't know where our own base is, then do nothing
        # However, this action should never fire unless we do know where our base is
        if self.PosInfo.OwnBasePos == None:
            print "Don't know where own base is!"
            return 1
        
        def send_getpath():
            print "in send_getpath"
            if not utilityfns.is_previous_message(self.agent.Bot, ("GETPATH", {"Location" : self.PosInfo.OwnBasePos, "Id" : self.PathHomeID})):
                self.agent.Bot.send_message("GETPATH", {"Location" : self.PosInfo.OwnBasePos, "Id" : self.PathHomeID}) # the ID allows us to match requests with answers
                print "sent GETPATH"
            else:
                print "GETPATH already sent"
        
        # if we haven't already got a list of path nodes to follow then send the GETPATH message
        # to try and mitigate the problem of pathhome being cleared part way through this, we assign
        # the relevant value to a variable and then use that throughout, so the array is checked as infrequently as possible
        # it's not an ideal fix though!
        if self.PosInfo.PathHome == []:
            send_getpath()
        else:
            if not self.to_known_location(self.PosInfo.PathHome, DistanceTolerance):
                print "DT check failed, tailing"
                self.PosInfo.PathHome = utilityfns.tail(self.PosInfo.PathHome)
                if self.PosInfo.PathHome != []:
                    print "tail not empty"
                    PathLoc = self.PosInfo.PathHome[0]
                    self.send_runto_or_strafe_to_location(PathLoc)
                else:
                    send_getpath()
    
        #before we return, send a checkreach command about the current navpoint.  That way the list can be recreated if it becomes incorrect
        if self.PosInfo.PathHome != [] and self.PosInfo.PathHome != None:
            self.agent.Bot.send_message("CHECKREACH", {"Location" : self.PosInfo.PathHome[0], "Id" : self.ReachPathHomeID, "From" : self.agent.Bot.botinfo["Location"]})
        print "about to return from to_own_base"
    
    # runs to the enemy's base by getting a list of navpoints showing the way there
    def to_enemy_base(self):
        print "to_enemy_base"
        DistanceTolerance = 30
        # If we don't know where the base is, then do nothing
        # However, this action should never fire unless we do know where it is
        if self.PosInfo.EnemyBasePos == None:
            print "Don't know where enemy base is!"
            return 1
        
        def send_getpath():
            print "in send_getpath"
            utilityfns.send_if_not_prev(self.agent.Bot, ("GETPATH", {"Location" : self.PosInfo.EnemyBasePos, "Id" : self.PathToEnemyBaseID}))
        
        # if we haven't already got a list of path nodes to follow then send the GETPATH message
        # to try and mitigate the problem of pathhome being cleared part way through this, we assign
        # the relevant value to a variable and then use that throughout, so the array is checked as infrequently as possible
        # it's not an ideal fix though!
        if self.PosInfo.PathToEnemyBase == []:
            send_getpath()
        else:
            if not self.to_known_location(self.PosInfo.PathToEnemyBase, DistanceTolerance):
                print "DT check failed, tailing"
                self.PosInfo.PathToEnemyBase = utilityfns.tail(self.PosInfo.PathToEnemyBase)
                if self.PosInfo.PathToEnemyBase != []:
                    print "tail not empty"
                    PathLoc = self.PosInfo.PathToEnemyBase[0]
                    self.send_runto_or_strafe_to_location(PathLoc)
                else:
                    send_getpath()
    
        #before we return, send a checkreach command about the current navpoint.  That way the list can be recreated if it becomes incorrect
        if self.PosInfo.PathToEnemyBase != [] and self.PosInfo.PathToEnemyBase != None:
            self.agent.Bot.send_message("CHECKREACH", {"Location" : self.PosInfo.PathToEnemyBase[0], "Id" : self.ReachPathToEnemyBaseID, "From" : self.agent.Bot.botinfo["Location"]})
        print "about to return from to_enemy_base"
    
    # returns 1 and sends a runto message for the provided location if the DistanceTolerance check passes
    # otherwise returns 0
    def to_known_location(self, Location, DistanceTolerance):
        if len(Location) == 0:
            return 1 # even though we failed, we return 1 so that it doesn't tail the list
        # to first point in current list, if we're not already there
        Location0 = Location[0]
        (HX, HY, HZ) = utilityfns.location_string_to_tuple(Location0)
        (SX, SY, SZ) = utilityfns.location_string_to_tuple(self.agent.Bot.botinfo["Location"])
        if utilityfns.find_distance((HX, HY), (SX, SY)) > DistanceTolerance:
            print "DistanceTolerance check passed"
            
            print "About to send RUNTO to",
            print Location0
            print "Current location",
            print self.agent.Bot.botinfo["Location"]
            
            self.send_runto_or_strafe_to_location(Location0, 0)
            # was
            #if not utilityfns.is_previous_message(self.agent.Bot, ("RUNTO", {"Location" : PathLoc})):
            #    self.agent.Bot.send_message("RUNTO", {"Location" : PathLoc})
            #    print "Running to " + PathLoc
            return 1
        else:
            return 0
        
    #not used at present (18/2/2005)
    def inch(self):
        # just add a bit to the x value
        print "in inch"
        (SX, SY, SZ) = utilityfns.location_string_to_tuple(self.agent.Bot.botinfo["Location"])
        NewLocTuple = (SX + 150, SY, SZ)
        self.send_runto_or_strafe_to_location(utilityfns.location_tuple_to_string(NewLocTuple))
  
    # just because something was reachable the last time we knew about it doesn't mean it still is
    #def expire_reachable_info(self):
    #    if self.PosInfo.OurFlagInfo != {} and self.PosInfo.OurFlagInfo.has_key("Reachable"):
    #        self.PosInfo.OurFlagInfo["Reachable"] = "0"
    #    if self.PosInfo.EnemyFlagInfo != {} and self.PosInfo.EnemyFlagInfo.has_key("Reachable"):
    #        self.PosInfo.EnemyFlagInfo["Reachable"] = "0"
    #    
    #    self.PosInfo.TooCloseForPath = 0
    
    # === OTHER FUNCTIONS ===
    
    # checks the previous sent message against the provided one, returning 1 if they match
    # now replaced by is_previous_message(bot, Msg) in utilityfns
    #def is_previous_message(self, Msg):
    #    if self.agent.Bot.sent_msg_log == None or \
    #    len(self.agent.Bot.sent_msg_log) == 0 or \
    #    self.agent.Bot.sent_msg_log[-1] != Msg:
    #        return 0
    #    return 1
    
    # updates the flag positions in PositionsInfo
    # also updates details of bases, if relevant info sent
    # the position of a flag is how we determine where the bases are
    def receive_flag_details(self, values):
        #print "f",
        #print values["Reachable"]
        print 'in receive_flag_details'
        print "\n".join(["%s=%s" % (k, v) for k, v in values.items()])
        if self.agent.Bot.botinfo == {}: #if botinfo is {}, we can't yet set anything
            return
        
        #print "in receive_flag_details.  Values are:"
        #print values
        
        #set flag stuff
        OurTeam = self.agent.Bot.botinfo["Team"]
        if values["Team"] == OurTeam:
            self.PosInfo.OurFlagInfo = values
            #print "our flag"
        else:
            self.PosInfo.EnemyFlagInfo = values
            #print "enemy flag"
        
        # now set base stuff if appliable
        if values["State"] == "home":
            if values["Team"] == self.agent.Bot.botinfo["Team"]:
                self.PosInfo.OwnBasePos = values["Location"]
            else:
                self.PosInfo.EnemyBasePos = values["Location"]
                #print "enemy base at",
                #print self.PosInfo.EnemyBasePos
                #print "self.PosInfo.EnemyBasePos has type",
                #print type(self.PosInfo.EnemyBasePos)
    
    # if the 'ID' key is PathHome then it tells the bot how to get home.
    # we need to turn the dictionary into a list, ordered by key ('0' ... 'n')
    # at present other IDs are ignored
    def receive_pth_details(self, ValuesDict):
        if not ValuesDict.has_key("ID"):
            return
        elif ValuesDict["ID"] == self.PathHomeID:
            self.PosInfo.PathHome =  utilityfns.nav_point_dict_to_ordered_list(ValuesDict)
        elif ValuesDict["ID"] == self.PathToEnemyBaseID:
            self.PosInfo.PathToEnemyBase =  utilityfns.nav_point_dict_to_ordered_list(ValuesDict)
            
        # if there's no 0 key we're being given an empty path, so set TooCloseForPath to the current time
        # so that we can check how old the information is later on
        if not ValuesDict.has_key("0"):
            self.PosInfo.TooCloseForPath = current_time()
        else:
            self.PosInfo.TooCloseForPath = 0
            
    
    # used in validating the bot's path home or to the enemy flag
    # if the thing has the right ID, then clear the relevant path if it's not reachable
    def receive_rch_details(self, ValuesDict):
        print "in receive_rch_details"
        if not ValuesDict.has_key("ID"):
            return
        elif ValuesDict["ID"] == self.ReachPathHomeID and ValuesDict["Reachable"] == "False":
            self.PosInfo.PathHome = []
            print "Cleared PathHome"
        elif ValuesDict["ID"] == self.ReachPathToEnemyBaseID and ValuesDict["Reachable"] == "False":
            self.PosInfo.PathToEnemyBase = []
            print "Cleared PathToEnemyBase"
        
    # if the combatinfo class specifies that we need to remain focused on a player, send a relevant strafe command
    # to move to the provided location.  Otherwise, a runto
    def send_runto_or_strafe_to_location(self, Location, PerformPrevCheck = 1):
        #expire focus id info if necessary FA 
        if self.agent.Combat.CombatInfo.KeepFocusOnID != None and self.agent.Combat.CombatInfo.has_focus_id_expired(): 
            self.agent.Combat.CombatInfo.expire_focus_id()

            # Seems out of place, need to find a better way of doing this FA
            self.agent.Bot.send_message("STOPSHOOT", {}) # no-one to focus on
    
        if self.agent.Combat.CombatInfo.KeepFocusOnID != None:
            Message = ("STRAFE", {"Location" : Location, "Target": self.agent.Combat.CombatInfo.KeepFocusOnID})
            if PerformPrevCheck:
                utilityfns.send_if_not_prev(self.agent.Bot, Message)
            else:
                self.agent.Bot.send_message(Message[0], Message[1])
            
        else:
            Message = ("RUNTO", {"Location" : Location})
            if PerformPrevCheck:
                utilityfns.send_if_not_prev(self.agent.Bot, Message)
            else:
                self.agent.Bot.send_message(Message[0], Message[1])
                print "have just sent",
                print Message
                
    # clean-up after dying
    def receive_die_details(self, ValuesDict):
        self.PosInfo.PathHome = []
        self.PosInfo.PathToEnemyBase = []
        self.PosInfo.VisitedNavPoints = [] # this is new
        self.PosInfo.OurFlagInfo = {}
        self.PosInfo.EnemyFlagInfo = {}
                
    
