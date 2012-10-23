using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys;
using POSH_sharp.sys.annotations;
//import utilityfns

namespace Posh_sharp_examples.BODBot
{
    public class CombatBehaviour : Behaviour
    {
        private string [] senses;
        private string [] actions;
        private CombatInfo info;

        public CombatBehaviour(AgentBase agent)
            :base(agent,new string[] {"ShootEnemyCarryingOurFlag",
                            "RunToTnemyCarryingOurFlag",
                            "FaceAttacker", "SetAttacker", "ShootAttacker"},
                        new string[] {"SeeEnemyWithOurFlag",
                            "OurFlagOnGround", "EnemyFlagOnGround",
                            "IncomingProjectile",
                            "TakenDamageFromSpecificPlayer",
                            "TakenDamage", "IsRespondingToAttack"})
        {
            info = new CombatInfo();
        }

        /*
         * 
         * SENSES
         * 
         */
        [ExecutableSense("SeeEnemyWithOurFlag")]
        public bool SeeEnemyWithOurFlag()
        {
            // print "in see_enemy_with_our_flag sense"
            if (this.agent.getBehaviour("Bot").)
            return false;
        }

    }
}
   
    # === SENSES ===
    
    def see_enemy_with_our_flag(self):
        #print "in see_enemy_with_our_flag sense"
        if len(self.agent.Bot.view_players) == 0:
            #print "  no players visible"
            return 0
        
        #else check through every player we can see to check whether they're the one holding our flag
        players = self.agent.Bot.view_players.values()
        for CurrentPlayer in players:
            #print CurrentPlayer
            if CurrentPlayer["Id"] == self.CombatInfo.HoldingOurFlag:
                print "  can see the player holding our flag"
                self.CombatInfo.HoldingOurFlagPlayerInfo = CurrentPlayer
                return 1
        #print "  cannot see the player holding our flag (",
        #print self.CombatInfo.HoldingOurFlag,
        #print ")"
        return 0
        
    def our_flag_on_ground(self):
        if self.agent.Movement.PosInfo.has_our_flag_info_expired():
            self.agent.Movement.PosInfo.expire_our_flag_info()
            
        if self.agent.Movement.PosInfo.OurFlagInfo == {}:
            return 0
        else:
            # in case the flag was returned but we didn't actually see it happen
            if not self.agent.Bot.gameinfo.has_key("EnemyHasFlag"):
                self.agent.Movement.PosInfo.OurFlagInfo["State"] = "home"
            
            if self.agent.Movement.PosInfo.OurFlagInfo["State"].lower() == "dropped":
                #print "our flag is dropped!"
                return 1
        return 0
        
    def enemy_flag_on_ground(self):
        if self.agent.Movement.PosInfo.has_enemy_flag_info_expired():
            self.agent.Movement.PosInfo.expire_enemy_flag_info()
        
        # Made simpler FA
        # By adding self.agent.Movement.PosInfo.EnemyFlagInfo["Reachable"] == "True" it has semi fixed the problem of the bot
        # standing still after it has picked up the flag off the ground and dropped it off at base.
        # This is because Reachable set to 0 on expiry of EnemyFlagInfo FA.
        if self.agent.Movement.PosInfo.EnemyFlagInfo != {} and self.agent.Movement.PosInfo.EnemyFlagInfo["State"].lower() == "dropped" and self.agent.Movement.PosInfo.EnemyFlagInfo["Reachable"] == "True":
            return 1
        return 0
        
    def incoming_projectile(self):
        if self.CombatInfo.has_projectile_details_expired():
            self.CombatInfo.expire_projectile_details()
    
        if self.CombatInfo.ProjectileDetails != None:
            print "incoming-projectile returning 1"
            return 1
        return 0
    
    def taken_damage_from_specific_player(self):
        #expire damage info if necessary FA
        if self.CombatInfo.DamageDetails != None and self.CombatInfo.has_damage_info_expired():
            self.CombatInfo.expire_damage_info()
            
        #expire focus id info if necessary FA 
        if self.CombatInfo.KeepFocusOnID != None and self.CombatInfo.has_focus_id_expired(): 
            self.CombatInfo.expire_focus_id()
            self.agent.Bot.send_message("STOPSHOOT", {}) # no-one to focus on
        
        #expire focus location info if necessary FA 
        if self.CombatInfo.KeepFocusOnLocation != None and self.CombatInfo.has_focus_location_expired(): 
            self.CombatInfo.expire_focus_location()
            self.agent.Bot.send_message("STOPSHOOT", {}) # no-one to focus on
                    
        if self.CombatInfo.DamageDetails != None and self.CombatInfo.DamageDetails.has_key("Instigator"):
            print "taken_damage_from_specific_player returning 1"
            return 1
        #alternatively, even if we don't know who shot us this time, we may know from another recent attack
        elif self.CombatInfo.KeepFocusOnLocation != None:
            return 1
        else:
            return 0
    
    # expire damage info if necassary FA
    def taken_damage(self):
        if self.CombatInfo.DamageDetails != None:
            if self.CombatInfo.has_damage_info_expired():
                self.CombatInfo.expire_damage_info()
                return 0
            return 1
        return 0      
            
    #def taken_damage_from_specific_player(self):
        #if self.CombatInfo.DamageDetails != None and self.CombatInfo.DamageDetails.has_key("Instigator"):
            #print "taken_damage_from_specific_player returning 1"
            #return 1
        # alternatively, even if we don't know who shot us this time, we may know from another recent attack
        #elif self.CombatInfo.KeepFocusOnLocation != None:
            #return 1
        #else:
            #return 0
            
    #def taken_damage(self):
        #if self.CombatInfo.DamageDetails != None:
            #return 1
        #return 0
        
    # returns 1 if we're already responding to the most recent attack
    # At present just test against KeepFocusOnID.  However, that doesn't 100% guarantee that we've started shooting,
    # just that we know who we ought to shoot.  For now, however, I will use this check.
    def is_responding_to_attack(self):
        #expire focus id info if necessary FA 
        if self.CombatInfo.KeepFocusOnID != None and self.CombatInfo.has_focus_id_expired(): 
            self.CombatInfo.expire_focus_id()
            self.agent.Bot.send_message("STOPSHOOT", {}) # no-one to focus on
    
        if self.CombatInfo.KeepFocusOnID != None:
            return 1
        else:
            return 0
            
    # === ACTIONS ===
    
    def shoot_enemy_carrying_our_flag(self):
        print "in secof"
        if self.CombatInfo.HoldingOurFlag != None and self.CombatInfo.HoldingOurFlagPlayerInfo != None:
            Target = self.CombatInfo.HoldingOurFlag
            Location = self.CombatInfo.HoldingOurFlagPlayerInfo["Location"]
            self.agent.Bot.send_message("SHOOT", {"Target" : Target, "Location" : Location})
        return 1
            
    def run_to_enemy_carrying_our_flag(self):
        print "in rtecof very start"
        if self.CombatInfo.HoldingOurFlag != None and self.CombatInfo.HoldingOurFlagPlayerInfo != None:
            print "in rtecof, past initial if"
            
            #Target = self.CombatInfo.HoldingOurFlag
            #if not utilityfns.is_previous_message(self.agent.Bot, ("RUNTO", {"Target" : Target})):
            #    self.agent.Bot.send_message("RUNTO", {"Target" : Target})
            #    print "running after enemy"
            
            Location = self.CombatInfo.HoldingOurFlagPlayerInfo["Location"]
            if not utilityfns.is_previous_message(self.agent.Bot, ("RUNTO", {"Location" : Location})):
                self.agent.Bot.send_message("RUNTO", {"Location" : Location})
            print "running after enemy"
        return 1

    #def set_tried_to_find_attacker(self):
    #    self.CombatInfo.TriedToFindAttacker = 1
        
        
    # if we can see the player currently, store his ID so e.g. runtos will be replaced by strafes to keep him in focus
    # and issue a turnto command
    def face_attacker(self):
        print "in face_attacker"
        
        #expire focus id info if necessary FA 
        if self.CombatInfo.KeepFocusOnID != None and self.CombatInfo.has_focus_id_expired(): 
            self.CombatInfo.expire_focus_id()
            self.agent.Bot.send_message("STOPSHOOT", {}) # no-one to focus on
        
        #expire focus location info if necessary FA 
        if self.CombatInfo.KeepFocusOnLocation != None and self.CombatInfo.has_focus_location_expired(): 
            self.CombatInfo.expire_focus_location()
            self.agent.Bot.send_message("STOPSHOOT", {}) # no-one to focus on
        
        if self.CombatInfo.KeepFocusOnLocation == None and self.CombatInfo.KeepFocusOnID == None:
            return 1

        if self.CombatInfo.KeepFocusOnID == None: #just provide location
            Location = self.CombatInfo.KeepFocusOnLocation
            Msg = ("TURNTO", {"Location" : Location})
            utilityfns.send_if_not_prev(self.agent.Bot, Msg)
        else:
            (Target, Timestamp) = self.CombatInfo.KeepFocusOnID
            (Location, Timestamp) = self.CombatInfo.KeepFocusOnLocation
            self.agent.Bot.send_message("TURNTO", {"Target" : Target})
        return 1
    
    # sets the attacker (i.e. the keepfocuson one) to be the first enemy player we have seen
    # or the instigator of the most recent damage, if we know who that is
    def set_attacker(self):
        print "in set_attacker"
        #if self.CombatInfo.DamageDetails != None and has_damage_info_expired(): self.CombatInfo.DamageDetails = None
        def find_enemy_in_view():
            # work through who we can see, looking for an enemy
            OurTeam = self.agent.Bot.botinfo["Team"]
            print "OurTeam:",
            print OurTeam
            Players = self.agent.Bot.view_players.values()
            for CurrentPlayer in Players:
                if CurrentPlayer["Team"] != OurTeam:
                    # Turned KeepFocusOnID in to a tuple with the current_time as a timestamp FA
                    self.CombatInfo.KeepFocusOnID = (CurrentPlayer["Id"], current_time())
                    self.CombatInfo.KeepFocusOnLocation = (CurrentPlayer["Location"], current_time())
                    return 1
        
        if len(self.agent.Bot.view_players) == 0 or self.agent.Bot.botinfo == {}: #if botinfo is {}, we can't yet set anything
            return 1
        else:
            # expire damage info if necessary FA
            if self.CombatInfo.DamageDetails != None and self.CombatInfo.has_damage_info_expired():
                self.CombatInfo.expire_damage_info()
            if self.CombatInfo.DamageDetails != None and self.CombatInfo.DamageDetails.has_key("Instigator"):
                InstID = self.CombatInfo.DamageDetails["Instigator"]
                if self.agent.Bot.view_players.has_key(InstID):
                    # set variables so that other commands will keep him in view
                    # Turned KeepFocusOnID in to a tuple with the current_time as a timestamp FA
                    self.CombatInfo.KeepFocusOnID = (InstID, current_time())
                    self.CombatInfo.KeepFocusOnLocation = (self.agent.Bot.view_players[InstID]["Location"], current_time())
                else:
                    find_enemy_in_view()
            else:
                find_enemy_in_view()
            return 1
        
    def shoot_attacker(self):
        print "in shoot_attacker"
        
        #expire focus id info if necessary FA 
        if self.CombatInfo.KeepFocusOnID != None and self.CombatInfo.has_focus_id_expired(): 
            self.CombatInfo.expire_focus_id()
            self.agent.Bot.send_message("STOPSHOOT", {}) # no-one to focus on
        
        #expire focus location info if necessary FA 
        if self.CombatInfo.KeepFocusOnLocation != None and self.CombatInfo.has_focus_location_expired(): 
            self.CombatInfo.expire_focus_location()
            self.agent.Bot.send_message("STOPSHOOT", {}) # no-one to focus on
        
        if self.CombatInfo.KeepFocusOnLocation == None:
            return 1

        if self.CombatInfo.KeepFocusOnID == None: #just provide location
            (Location, Timestamp) = self.CombatInfo.KeepFocusOnLocation
            if not utilityfns.is_previous_message(self.agent.Bot, ("SHOOT", {"Location" : Location})):
                self.agent.Bot.send_message("SHOOT", {"Location" : Location})
        else:
            (Target, Timestamp) = self.CombatInfo.KeepFocusOnID
            (Location, Timestamp) = self.CombatInfo.KeepFocusOnLocation
            if not utilityfns.is_previous_message(self.agent.Bot, ("SHOOT", {"Target" : Target, "Location" : Location})):
                self.agent.Bot.send_message("SHOOT", {"Target" : Target, "Location" : Location})
        return 1
        
    # === OTHER FUNCTIONS ===
    
    def receive_flag_details(self, values):
        # if its status is "held", update the CombatInfoClass to show who's holding it
        # otherwise, set that to None as it means no-one is holding it
        
        #print "in rfd"
        #print values
        
        if self.agent.Bot.botinfo == {}: #if botinfo is {}, we can't yet set anything
            return
        
        OurTeam = self.agent.Bot.botinfo["Team"]
        #print "OurTeam is of type",
        #print type(OurTeam),
        #print " and value is",
        #print OurTeam
        #print "values[\"Team\"] is ",
        #print values["Team"]
        
        if values["Team"] == OurTeam:
            if values["State"].lower() == "held":
                #print "setting holder"
                self.CombatInfo.HoldingOurFlag = values["Holder"]
            else:
                #print "not being held"
                self.CombatInfo.HoldingOurFlag = None
                self.CombatInfo.HoldingOurFlagPlayerInfo = None
                
    def receive_prj_details(self, valuesdict):
        print "received details of incoming projectile!"
        print valuesdict
        self.CombatInfo.ProjectileDetails = valuesdict
        
    def receive_dam_details(self, valuesdict):
        self.CombatInfo.DamageDetails = valuesdict
        
    # handle details about a player (not itself) dying
    # remove any info about that player from CombatInfo
    def receive_kil_details(self, ValuesDict):
        print "receive_kil_details",
        print ValuesDict
        print "-----"
        print self.CombatInfo.HoldingOurFlag
        if ValuesDict["Id"] == self.CombatInfo.HoldingOurFlag:
            self.CombatInfo.HoldingOurFlag = None
            self.CombatInfo.HoldingOurFlagPlayerInfo = None
            self.agent.Bot.send_message("STOPSHOOT", {})

        if self.CombatInfo.KeepFocusOnID != None:
            (ID, TimeStamp) = self.CombatInfo.KeepFocusOnID
            if ValuesDict["Id"] == ID:
                self.CombatInfo.expire_focus_id()
                self.CombatInfo.expire_focus_location()
                self.agent.Bot.send_message("STOPSHOOT", {})
            
    
    # clean-up after dying
    def receive_die_details(self, ValuesDict):
        self.CombatInfo.expire_damage_info()
        self.CombatInfo.expire_focus_id()
        self.CombatInfo.expire_focus_location()
        self.agent.Bot.send_message("STOPSHOOT", {}) # no-one to focus on
    
class CombatInfoClass:
    def __init__(self):
        self.HoldingOurFlag = None # the ID of the player holding our flag
        self.HoldingOurFlagPlayerInfo = None # details about that player
        
        self.ProjectileDetails = None
        self.DamageDetails = None
        self.KeepFocusOnID = None
        self.KeepFocusOnLocation = None
        
        #self.TriedToFindAttacker = 0
        
    # Checks the timestamp against current time less lifetime of damagedetails FA
    def has_damage_info_expired(self, lsecs = 5):
        if self.DamageDetails != None and self.DamageDetails["timestamp"] < (current_time() - lsecs):
            return 1
        return 0
        
    # not the usual sort of action, but ensures that details about e.g. damage taken doesn't reside forever and inform decisions too far into the future
    def expire_damage_info(self):
        self.DamageDetails = None
        return 1
        
    # Checks the timestamp against current time less lifetime of focus_id FA
    def has_focus_id_expired(self, lsecs = 15):
        if self.KeepFocusOnID != None:
            (ID, timestamp) = self.KeepFocusOnID
            if timestamp < (current_time() - lsecs):
                return 1
        return 0
        
    # Split expire_focus_info in to two methods for better accuracy FA 
    def expire_focus_id(self):
        self.KeepFocusOnID = None
    
    # Checks the timestamp against current time less lifetime of focus_id FA
    def has_focus_location_expired(self, lsecs = 15):
        if self.KeepFocusOnLocation != None:
            (location, timestamp) = self.KeepFocusOnLocation
            if timestamp < (current_time() - lsecs):
                return 1
        return 0
    
    # Split expire_focus_info in to two methods for better accuracy FA  
    def expire_focus_location(self):
        self.KeepFocusOnLocation = None
        
    def has_projectile_details_expired(self, lsecs = 2):
        if self.ProjectileDetails != None and self.ProjectileDetails["timestamp"] < (current_time() - lsecs):
            return 1
        return 0
    
    def expire_projectile_info(self):
        self.ProjectileDetails = None
        return 1
