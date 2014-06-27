using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys.scheduled
{
    public class Agent : AgentBase
    {
        /**
         * 
         * These constants control the amount of sleep() between runs. They work in a
         * logarithmic fashion when approaching the processor limit.
         * Default # of seconds to delay between each driver run. This is a hard limit
         * of the number of runs that can happen in one second.
         *                    1/DRIVER_DELAY * DELAY_MULTIPLIER = Hz
         * Each agent can set own self.delay. Set to 0 to disable limit. Generally
         * not accurate over approx. 100Hz with a multiplier of 1.
         * DELAY_MULTIPLIER can not be 0. If 0, it will cause divide by 0 error.
         * Time consumming bb checks may further lower the frequency of drive runs.
         */
        public const float DRIVER_DELAY = 0.1f;
        //#DRIVER_DELAY = 0
        public const float DELAY_MULTIPLIER = 0.5f;

        private float driverDelay;
        private float delayMultiplier;

        // drive collection results
        public const int DRIVEFOLLOWED = 0;
        public const int DRIVEWON = 1;
        public const int DRIVELOST = -1;


        protected internal DriveCollection dc;

		public Agent(string library, string plan, Dictionary<Tuple<string,string>,object> attributes)
			: this(library,plan,attributes,null,DRIVER_DELAY,DELAY_MULTIPLIER)
		{}

        public Agent(string library, string plan, Dictionary<Tuple<string,string>,object> attributes, World world,
            float driverdelay, float delaymultiplier)
            : base(library,plan,attributes,world)
        {
            // timing and execution variables
            driverDelay = driverdelay;
            delayMultiplier = delaymultiplier;
        }
    }
}

//# ||TODO|| OK if the plan file isn't too large. Implement a beter
//# parser, probably using parse trees.

//def dict(sequence):
//     resultDict = {}
//     for key, value in sequence:
//         resultDict[key] = value
//     return resultDict




//# The agent instance keeps track of all the objects while the
//# script is read in.
//class Agent(AgentBase):

    
//    def _loadPlan(self, planfile):
//        """Loads the plan and creates the drive collection tree.
        
//        The method parses the plan file, and then links the created objects
//        into the tree.
        
//        @param planfile: Filename of the plan file that is loaded.
//        @type planfile: string
//        """
//        # initialise scheduler and blackboard
//        # I'm not sure if they need to be there before the tree is created.
//        # If they don't, then the following two lines can be moved into the
//        # constructor, directly after calling AgentBase.__init__(...)
//        self.schedule = Schedule(agent = self)
//        self.blackboard = Blackboard(agent = self)
//        # sets up dictionaries to store action pattern and competences,
//        # drive elements, competence elements
//        self.ap_dict = {}
//        self.comp_dict = {}
//        self.drive_element_dict = {}
//        self.competence_element_dict = {}
//        # This keeps the drive collection while parsing. self.create_tree()
//        # links up all of the drive-roots in the drive elements and
//        # stores the finished tree here
//        self.drive_collection = None
//        # lead plan file and create drive collection tree
//        self.read_file(planfile)
//        self.create_tree()
        
//    def add_action_pattern(self, name, action_pattern):
//        self.log.info("Adding Action Pattern - %s" % name)
//        self.ap_dict[name] = action_pattern

//    def add_competence(self, name, competence):
//        self.log.info("Adding Competence - %s" % name)
//        self.comp_dict[name] = competence

//    def add_competence_element(self, name, competence_element):
//        self.log.info("Adding Competence Element - %s" % name)
//        self.competence_element_dict[name] = competence_element

//    def add_drive_element(self, name, drive_element):
//        self.log.info("Adding Drive Element - %s" % name)
//        self.drive_element_dict[name] = drive_element
        
//    def get_action_pattern(self, name):
//        if self.ap_dict.has_key(name):
//            self.log.info("Done Retrieving Action Pattern - %s" % name)
//            return self.ap_dict[name]
//        else:
//            self.log.info("Cannot Retrieve Action Pattern - %s" % name)
//            return None
        
//    def get_competence(self, name):
//        if self.comp_dict.has_key(name):
//            self.log.info("Done Retrieving Competence - %s" % name)
//            return self.comp_dict[name]
//        else:
//            self.log.info("Cannot Retrieve Competence - %s" % name)
//            return None
        
//    def get_drive_collection(self):
//        self.log.info("Returning Drive Collection")
//        return self.drive_collection
        
//    def get_drive_collection_name(self):
//        self.log.info("Returning Drive Collection Name")
//        if isinstance(self.drive_collection, Drive_Collection):
//            return self.drive_collection.name
//        else:
//            return None
        
//    def get_drive_element(self, name):
//        if self.drive_element_dict.has_key(name):
//            self.log.info("Done Retrieving Drive Element - %s" % name)
//            return self.drive_element_dict[name]
//        else:
//            self.log.info("Cannot Retrieve Drive Element - %s" % name)
//            return None

//    def get_competence_element(self, name):
//        if self.competence_element_dict.has_key(name):
//            self.log.info("Done Retrieving Competence Element- %s" % name)
//            return self.competence_element_dict[name]
//        else:
//            self.log.info("Cannot Retrieve Competence Element - %s" % name)
//            return None

//    def find_action_element(self, name):
//        self.log.info("Compound Retrieval find_action_element() - %s" % name)
//        # using behaviour dictionary instead of previous get_act()
//        try:
//            return self._bdict.getAction(name)
//        except NameError:
//            pass
//        # from here its the same as before
//        if self.get_action_pattern(name):
//            return self.get_action_pattern(name)
//        elif self.get_competence(name):
//            return self.get_competence(name)
//        elif self.get_drive_collection_name() == name:
//            return self.drive_collection
//        else:
//            # Found Nothing
//            self.log.info("Failed find_action_element() - %s" % name)
//            return None

//    def find_element(self, name):
//        self.log.info("Compound Retrieval find_element() - %s" % name)
//        # using behaviour dictrionary instead of previous get_act() / get_sense()
//        try:
//            return self._bdict.getAction(name)
//            return self._bdict.getSense(name)
//        except NameError:
//            pass
//        # from here its the same as before
//        if self.get_action_pattern(name):
//            return self.get_action_pattern(name)
//        elif self.get_competence(name):
//            return self.get_competence(name)
//        elif self.get_competence_element(name):
//            return self.get_competence_element(name)
//        elif self.get_drive_collection_name() == name:
//            return self.self.drive_collection
//        elif self.get_drive_element(name):
//            return self.get_drive_element(name)
//        else:
//            # Found Nothing
//            self.log.info("Failed find_element() - %s" % name)
//            return None
        
//    def create_tree(self):
//        def validate_action_pattern(ap):
//            self.log.info("Validating Action Pattern - %s" % ap.name)
//            # Call the elements validation routine immediately
//            # It will also take care of the list depth (ie. sub-sub-lists)
//            ap.elements = validate_ap_elements(ap.elements)
//            return ap

//        def validate_ap_elements(item, depth = 0):
//            if type(item) is ListType:
//                if len(item) > 0:
//                    if type(item[0]) is ListType:
//                        if depth >= 1:
//                            self.log.error( \
//                             "Action Pattern Elements exceed list depth (1)")
//                            return None # This should report an error
//                        rlist = []
//                        for x in item:
//                            rlist.append(validate_ap_elements(item, \
//                                                       depth = depth + 1))
//                        return rlist
//                    else:
//                        rlist = []
//                        for x in item:
//                            # If the first item is not a list, then that means
//                            # that this list should contain Sense objects or
//                            # action functions
//                            if type(x) is InstanceType \
//                                   and isinstance(x, Sense):
//                                # x.trigger = validate_action_pattern(
//                                #     x.trigger)
//                                rlist.append(x)
//                            else:
//                                # If the object isn't a Sense, then it must be a
//                                # placeholder for an action. Find it
//                                obj = self.find_element(x)
//                                if obj:
//                                    rlist.append(obj)
//                                else:
//                                    self.log.error( \
//                             "Cannot find action element - %s" % str(obj))
//                        return rlist

//            else:
//                self.log.error( \
//                    "Received single item when anticipating list")
//                return None # Not supposed to get single values should raise
//                            # error
            
//        def validate_competence(comp):
//            self.log.info("Validating Competence - %s" % comp.name)
//            # First validate the trigger (which is an action pattern)
//            comp.goal.trigger = validate_action_pattern(comp.goal.trigger)
//            # Then validate all elements
//            comp.elements = validate_comp_elements(comp.elements)
//            return comp
            
//        def validate_comp_elements(item, depth = 0):
//            if type(item) is ListType:
//                if len(item) > 0:
//                    if type(item[0]) is ListType:
//                        if depth >= 2:
//                            self.log.error( \
//                             "Competence Elements exceed list depth (2)")
//                            return None # This should report an error
//                        rlist = []
//                        for x in item:
//                            rlist.append(validate_comp_elements(x, \
//                                                            depth = depth + 1))
//                        return rlist
//                    else:
//                        rlist = []
//                        # The list should be of Objects, process each one
//                        for x in item:
//                            if type(x) is InstanceType \
//                                   and isinstance(x, Competence_Element):
//                                # This replaces the "action" attribute of the
//                                # Generic() instance in the action attribute
//                                # of the current comp. element. The string
//                                # is replaced with the actual first-class
//                                # function
//                                x.action.action = \
//                                       self.find_action_element( \
//                                    x.action.action)
//                                if x.action.action == None:
//                                    rlist.append(None)
//                                    # This means that error occured
//                                    # fetching the action
//                                    self.log.error( \
//                         "Unable to fetch action for the action instance of - %s" % \
//                                                        x)
//                                else:
//                                    rlist.append(x)
//                            else:
//                                rlist.append(None)
//                                self.log.error( \
//                                "Wrong type of object in competence elem list")
//                        return rlist
//                else:
//                    return [] # If empty list, return a empty list
//            else:
//                self.log.error( \
//                    "Received single item when anticipating list")
//                return None # Not supposed to get single values should raise
//                            # error
                            
//        def validate_drive_collection(drivec):
//            self.log.info("Validating Drive Collection")
//            # First validate the trigger (which is an action pattern)
            
//            drivec.goal.trigger = validate_action_pattern(drivec.goal.trigger)
//            # Then validate all elements
//            drivec.elements = validate_drive_elements(drivec.elements)
//            return drivec

//        def validate_drive_elements(item, depth = 0):
//            if type(item) is ListType:
//                if len(item) > 0:
//                    if type(item[0]) is ListType:
//                        if depth >= 2:
//                            self.log.error( \
//                             "Drive Elements exceed list depth (2)")
//                            return None
//                        rlist = []
//                        for x in item:
//                            rlist.append(validate_drive_elements(x, \
//                                                            depth = depth + 1))
//                        return rlist
//                    else:
//                        rlist = []
//                        # The list should be of Objects, process each one
//                        for x in item:
//                            if type(x) is InstanceType \
//                                   and isinstance(x, Drive_Element):
//                                # This replaces the "action" attribute of the
//                                # Generic() instance in the action attribute
//                                # of the current comp. element. The string
//                                # is replaced with the actual first-class
//                                # function
//                                x.trigger = validate_action_pattern(
//                                    x.trigger)
//                                x.drive_root.action = \
//                                       self.find_action_element( \
//                                    x.drive_root.action)
//                                if x.drive_root.action == None:
//                                    rlist.append(x)
//                                    # This means that error occured
//                                    # fetching the action
//                                    self.log.error( \
//                          "Unable to fetch action for the drive root of -" + \
//                                                        x.name )
//                                else:
//                                    rlist.append(x)
//                            else:
//                                rlist.append(None)
//                                self.log.error( \
//                                "Wrong type of object in drive elements list")
//                        return rlist
//                else:
//                    return [] # If empty list, return a empty list
//            else:
//                self.log.error( \
//                                "Received single item when anticipating list")
//                return None

//        #
//        # Start doing the grunt work and link up the objects
//        #

//        self.ap_dict = dict([(key , validate_action_pattern(value)) \
//                                    for key , value in self.ap_dict.items()])
//        self.comp_dict = dict([(key , validate_competence(value)) \
//                                for key , value in self.comp_dict.items()])
//        self.drive_collection = validate_drive_collection( \
//            self.drive_collection)

//        return self.drive_collection


//#   ###############################################################
//#   # This is the master command - Returns a drive collection     #
//#   # Reader.read_file                                            #
//#   ###############################################################
//    def read_file(self, filename):
//        self.log.info("Reading in Plan - %s" % filename)
//        planfile = open(filename)

//        # Takes in a string and strips out all the comments from
//        # or ; to newline. Then reformats the parentheses by
//        # adding whitespaces around them
//        def strip_comments(string):
//            # Remove everything from the comment symbol to the newline
//            r = re.compile('[\#\;].*(\n|\r\n)')
//            string = r.sub('', string)
        
//            # Remove the newlines
//            r2 = re.compile('(\n|\r\n)')
//            string = r2.sub('', string)
        
//            # Format the parens
//            r3 = re.compile('\(')
//            string = r3.sub(' ( ', string)
//            r4 = re.compile('\)')
//            string = r4.sub(' ) ', string)

//            # Returns the string as a list by spliting it at whitespace
//            return string.split()

//        # This function takes in a string and then for each expression
//        # enclosed in brackets creates a python list. It calls itself
//        # recursively in order to create lists within lists.
//        def read_list(input_list):
//            # Reverse the list ready for popping (python's pops from the end)
//            input_list.reverse()
//            return_list = []
//            temp_list = []
//            index = 0                                           

//            # When the loop encounters a opening bracket, tell it to store
//            # the items until it counters the bracket that closes it.
//            # Then pass the list of stored items into itself recusively
//            while input_list:
//                item = input_list.pop()
//                if item == '(':
//                    index = index + 1
//                    if index > 1:
//                        temp_list.append(item)
//                elif item == ')':
//                    index = index - 1
//                    if index == 0:
//                        return_list.append(read_list(temp_list))
//                        temp_list = []
//                    else:
//                        temp_list.append(item)
//                else:
//                    if index == 0:
//                        return_list.append(item)
//                    else:
//                        temp_list.append(item)
//            return return_list

//        def read_objects(input_list):

        
//#   #   #   ###########################################################
//#   #   #   # Action Pattern Functions Reader.read_file.read_objects  #
//#   #   #   ###########################################################

//            def read_action_pattern(item):
//                if len(item) != 4:
//                    self.log.error( \
//                      "Wrong Number of Arguments (AP name time elements) - %s" % str(item))
//                self.add_action_pattern(name = item[1],
//                                        action_pattern = return_action_pattern(
//                    name = item[1],
//                    timeout = self.fix_time(item[2]),
//                    timeout_rep = item[2],
//                    ap_guts = item[3]))

//            # When the item is in a sublist, pass it on as a Sense(),
//            # otherwise, it is just a act/comp/ap call
//            def read_ap_element(elem):
//                if type(elem) is ListType:
//                    return read_ap_sense(elem)
//                else:
//                    return elem

//            def read_ap_sense(item):
//                # print item
//                item.reverse()
//                name = item.pop()
//                # Returns a reference to a sense function
//                value = None
//                predicate = None
//                # instead of using get_sense()
//                try:
//                    sensor = self._bdict.getSense(name)
//                except:
//                    self.log.error( \
//                        "Sense Not Found - %s" % str(name))

//                # print len(item)
//                if len(item) > 0:
//                    value = item.pop()
//                    if len(item) > 0: # If there is a predicate
//                        predicate = item.pop()
//                        if predicate in ("eq", "="):
//                            predicate = "=="
//                        elif predicate in ("lt", "<"):
//                            predicate = "<"
//                        elif predicate in ("gt", ">"):
//                            predicate = ">"
//                        elif predicate in ("not", "!", "!="):
//                            predicate = "!="
//                        else:
//                            self.log.error( \
//              "Predicate %s invalid for Sense %s" % (str(predicate), name))
//                    else:
//                        predicate = "=="
//                    # Assign the comparision as a lambda function
//                return Sense(name = name,
//                             sensor = sensor,
//                             sense_value = value,
//                             sense_predicate = predicate,
//                             tag = name,
//                             command = "sense",
//                             action = "predicate",
//                             value = "value",
//                             agent = self)

//            def return_action_pattern(ap_guts,
//                                      name,
//                                      timeout,
//                                      timeout_rep):
//                return Action_Pattern(name = name,
//                                      ap_guts = ap_guts,
//                                      timeout = timeout,
//                                      timeout_rep = timeout_rep,
//                                      command = "element",
//                                      tag = name,
//                                      action = "posh",
//                                      elements = \
//                                          map(read_ap_element, ap_guts),
//                                      agent = self)

            
//#   #   #   ###########################################################
//#   #   #   # Competence Functions Reader.read_file.read_objects      #
//#   #   #   ###########################################################
//            def read_competence(item):
//                name = item[1]
//                self.log.info("Reading Competence - %s" % name)
//                timeout = self.fix_time(item[2])
//                timeout_rep = item[2]
//                ap_guts = item[3][1][:] # Make a copy of AP list (for goals)
//                elements = item [4][:]  # Make a copy of the elements
//                del(elements[0]) # Delete the "elements" word
//                self.add_competence(name = item[1],
//                                    competence = Competence(
//                    name = name,
//                    command = "request",
//                    tag = name,
//                    action = "posh",
//                    timeout_rep = timeout_rep,
//                    timeout = timeout,
//                    agent = self,
//                    goal = Competence_Element(competence = "see-action-tag",
//                                              action = Generic(name = str(name) + "_goal",
//                                                               command = "request",
//                                                               tag = str(name) +  "_goal",
//                                                               action = None,
//                                                               agent = self),
//                                              trigger = return_action_pattern(ap_guts = ap_guts,
//                                                                              name = str(name) + "_goal_trigger",
//                                                                              timeout = timeout,
//                                                                              timeout_rep = timeout_rep),
//                                              agent = self),
//                    elements = read_competence_elements(competence = name,
//                                                        elements = elements,
//                                                        timeout = timeout)))

//            # Do a recursive, multi-level descent into the list. Descend
//            # until a non-list is found and return it as an element
//            # Limit the level of descent to 2
//            def read_competence_elements(competence,
//                                         elements,
//                                         timeout,
//                                         depth = 0):
//                #print elements
//                if type(elements) is ListType:
//                    if type(elements[0]) is ListType:
//                        # If the first item is a list, then all of the items
//                        # are lists
//                        if depth >= 2:
//                            self.log.error( \
//                               "Competence Elements exceed list depth (2)")
//                            # We have a problem with sub-sub lists
//                            return None
//                        rlist =[]
//                        for x in elements:
//                            rlist.append(read_competence_elements(competence,
//                                                                  x,
//                                                                  timeout,
//                                                                  depth =
//                                                                  depth + 1))
//                        #print rlist
//                        return rlist
//                    else:
//                        # This list represents a competence element
//                        label = elements[0]
//                        trigger_ap = elements[1][1]
//                        action = elements[2]
//                        if len(elements) > 3:
//                            retries = int(elements[3]) # int() is new, was just = elements[3] (Sam, 17/2/2005)
//                        else:
//                            retries = -1
//                        robj = Competence_Element(competence = competence,
//                                                  ce_label = label,
//                                                  action = Generic(
//                            name = action,
//                            command = "request",
//                            tag = label,
//                            action = action,
//                            agent = self),
//                                                  retries = retries,
//                                                  trigger =
//                                                  return_action_pattern(
//                            ap_guts = trigger_ap,
//                            name = label + "_trigger",
//                            timeout = timeout,
//                            timeout_rep = trigger_ap),
//                                                  agent = self)
//                        self.add_competence_element(label, robj)
//                        return robj
//                else:
//                    self.log.error( \
//                               "Competence Element Incompatible (Expecting a list)")
//                    # Error: Elements format incompatible
                 
//            def read_drive_collection(item):
//                if item[0] == "RDC":
//                    realtime = 1
//                else:
//                    realtime = 0

//                name = item[1]
//                self.log.info("Reading Drive Collection - %s" % name)
//                goal = item[2]
//                elements = item[3]
//                del(elements[0]) # Strip off the item with the string "drives"
//                self.drive_collection = Drive_Collection(name = name,
//                                                         command = "request",
//                                                         realtime = realtime,
//                                                         tag = name,
//                                                         action = "posh",
//                                                         goal =
//                                                         Competence_Element(
//                    competence = "see_action_tag",
//                    action = Generic(name = str(name) + "goal",
//                                     command = "request",
//                                     tag = str(name) + "goal",
//                                     action = None,
//                                     agent = self),
//                    trigger = \
//                            return_action_pattern(ap_guts = goal[1],
//                                                  name = str(name) + \
//                                                  "_goal_trigger",
//                                                  timeout = 0,
//                                                  timeout_rep = None),
//                    agent = self),
//                                                         elements = \
//                            read_drive_elements(elements, name),
//                                                         agent = self)
                                                           
//            def read_drive_elements(elements, drive_collection, depth = 0):
//                if type(elements) is ListType:
//                    if type(elements[0]) is ListType:
//                        # If the first item is a list, then all of the items
//                        # are lists
//                        if depth >= 2:
//                            self.log.error( \
//                               "Drive Elements exceed list depth (2)")
//                            # We have a problem with sub-sub lists
//                            return None
//                        rlist =[]
//                        for x in elements:
//                            rlist.append(read_drive_elements(x,
//                                                             drive_collection,
//                                                             depth = depth+1))
//                        return rlist    
//                    else:
//                        # This list represents a drive element
//                        drive_name = elements[0]
//                        trigger_ap = elements[1][1]
//                        drive_root = elements[2]
//                        if len(elements) > 3:
//                            frequency_rep = elements[3]
//                            frequency = self.fix_time(frequency_rep)
//                        else:
//                            frequency_rep = None
//                            frequency = -1
//                        robj = Drive_Element(drive_collection \
//                                             = drive_collection,
//                                             drive_root_rep = drive_root,
//                                             drive_root =
//                                             Generic(
//                            name = drive_root,
//                            drive_name = drive_name,
//                            command = "request",
//                            tag = drive_name,
//                            action = drive_root,
//                            agent = self),
//                                             frequency = frequency,
//                                             frequency_rep = frequency_rep,
//                                             drive_name = drive_name,
//                                             trigger =
//                                             return_action_pattern(
//                            ap_guts = trigger_ap,
//                            name = drive_name + "_trigger",
//                            timeout = -1,
//                            timeout_rep = trigger_ap),
//                                             agent = self)
//                        self.add_drive_element(drive_name, robj)
//                        return robj
//                else:
//                    self.log.error( \
//                        "Drive Element Incompatible (Expecting a list)")
//                    # Error: Elements format incompatible                

//            # Parses the type of each element and calls the corresponding
//            # function to add the object to the cache
//            for item in input_list:
//                if type(item) is ListType:
//                    if item[0] == "C":
//                        read_competence(item)
//                    elif item[0] in ("DC", "RDC"):
//                        read_drive_collection(item)
//                    elif item[0] == "AP":
//                        read_action_pattern(item)
//                    elif type(item[0]) is ListType:
//                        read_objects(item)
//                    else:
//                        self.log.error( \
//                        "Token invalid (Expecting C, DC, RDC, AP) - %s" % \
//                                            str(item[0]))
//                        # Return Error
//                else:
//                    self.log.error( \
//                    "Invalid top-level item (Expecting a list) %s" % str(item))
//                    # Return Error

//            return 1

    
//        # Read in the file into a string, strip all the comments and
//        # format the string into a flat list of objects. This flat
//        # list is then passed to make_list which creates the list recursively
//        return read_objects(read_list(strip_comments(planfile.read())))

//    # Unifies the time representation into seconds
//    def fix_time(self, time_list):
//        if type(time_list) is ListType:
//            valuestr = time_list.pop()
//            value = float(valuestr)
//            unit = time_list.pop()
//            if value:
//                if unit in ("none", "absolute", "not_real_time"):
//                    return value
//                elif unit in ("minute", "minutes", "min"):
//                    return value * 60
//                elif unit in ("second", "seconds", "sec"):
//                    return value
//                elif unit in ("hertz", "hz"):
//                    return 1 / value
//                elif unit in ("per-minute", "pm"):
//                    return 60 / value
//                else:
//                    self.log.error( \
//                    "Invalid time unit - %s" % str(unit))
//            else:
//                # The time value is not a numeric
//                self.log.error( \
//                    "Invalid time value - Not a int or float - %s" % str(valuestr))
//        else:
//            # We didn't get a list pass in.
//            self.log.error( \
//                    "Invalid time list format - Not a list %s" % str(time_list))
    
//    def reset(self, waittime = 300):
//        """Prepares the agent for running the main loop by checking if the
//        behaviours are ready and resetting the agent's timer.
        
//        The waittime is the time allowed until the behaviours are getting
//        ready. It is the same as given to checkError(). By default, it is
//        set to 20 seconds.
        
//        @param waittime: Timout waiting for behaviours (see L{checkError()}).
//        @type waittime: int
//        @return: If the reset was successful.
//        @rtype: bool
//        """
//        if not AgentBase.reset(self, waittime):
//            return False
//        self.timestamp = 0
//        self.counter = 0
//        self.last_sec = 0
//        self.result = None
//        return True
    
//    def followDrive(self):
//        """Performes one loop through the drive collection.
//        """
//        if (self.result != "drive_lost") and (self.result != "drive_won"):
//            self.timestamp = current_time()

//            self.blackboard.check_bb()
//            self.result = self.driver(drive_collection = self.drive_collection,
//                        timestamp = self.timestamp)
//        else:
//            self.log.error("followDrive() called even though agent is done")

//    def _loop_thread(self):
//        if self.checkError(300) > 0:
//            self.log.error( \
//                    "Agent behavior is not ready and timed out. Stop execute.")
//            return False
//        ### For profiling
//        # counter2 = 0
//        ### For Hertz Counter ###
//        counter = 0
//        last_sec = int(current_time())
//        if self.drive_collection.realtime:
//            # Real-Time timestamps
//            result = None
            
//            # self.log.error("here!")
//            try:
//                while (result != "drive_lost") and (result != "drive_won") and \
//                      self._exec_loop and self.checkError(0) == 0:
                    
//                    # check for pause
//                    if self._loop_pause:
//                        # check ever 10th of a second
//                        while self._loop_pause:
//                            time.sleep(0.1)
//                    # check if stopLoop was called
//                    if not self._exec_loop:
//                        break
    
//                    timestamp = current_time()
//                    ################################
//                    #counter2 += 1
//                    #if counter2 > 10000:  # For Profiling
//                    #    self.exit()
//                    ################################
//                    # For Hz Counter
//                    counter += 1
//                    if int(timestamp) != last_sec:
//                        last_sec = int(timestamp)
//                        hertz = counter
//                        counter = 0
//                        self.log.debug("RT Hertz (Approx.)- %s" % repr(hertz))
//                        self.log.debug("BB Length - %s" % \
//                          str(len(self.blackboard._Blackboard__bb)))
//                        self.log.debug("Sched Length - %s" % \
//                          str(len(self.schedule._Schedule__schedule)))
//                    ################################
//                    self.log.debug( \
//                        "New RT Execute Run at - %s" % repr(timestamp))
//                    self.blackboard.check_bb()
//                    result = self.driver(drive_collection = self.drive_collection,
//                                timestamp = timestamp)
//                    ### To limit Hz
//                    if self.driver_delay and self.delay_multiplier:
//                        if not (counter % self.delay_multiplier):
//                            time.sleep(self.driver_delay)
//            except:
//                exc_type, exc_value, exc_traceback = sys.exc_info()
//                lines = traceback.format_exception(exc_type, exc_value, exc_traceback)
//                print ''.join('!! ' + line for line in lines)
//                #print 'Handling run-time error:', e
            
//        else:
//            # Incremented Timestamps
//            timestamp = 0
//            result = None

//            while (result != "drive_lost") and (result != "drive_won") and \
//                  self._exec_loop and self.checkError(0) == 0:
                
//                # check for pause
//                if self._loop_pause:
//                    # check ever 10th of a second
//                    while self._loop_pause:
//                        time.sleep(0.1)
//                # check if stopLoop was called
//                if not self._exec_loop:
//                    break
                
//                #self._exec_loop and self.bot.conn_ready: # was and 
//                timestamp = timestamp + 1
//                ################################
//                #counter2 += 1
//                #if counter2 > 10000:  # For Profiling
//                #    self.exit()
//                ################################
//                # For Hz Counter
//                now = current_time()
//                counter += 1
//                if int(now) != last_sec:
//                    last_sec = int(now)
//                    hertz = counter
//                    counter = 0
//                    self.log.debug("Iter. Hertz (Approx.)- %s" % repr(hertz))
//                    self.log.debug("BB Length - %s" % \
//                      str(len(self.blackboard._Blackboard__bb)))
//                    self.log.debug("Sched Length - %s" % \
//                      str(len(self.schedule._Schedule__schedule)))
//                ################################
//                self.log.debug( \
//                "New Iterative Execute Run at - %s" % repr(timestamp))
//                self.blackboard.check_bb()
//                result = self.driver(drive_collection = self.drive_collection,
//                            timestamp = timestamp)
//                ### To limit Hz
//                if self.driver_delay and self.delay_multiplier:
//                    if not (counter % self.delay_multiplier):
//                        time.sleep(self.driver_delay)
        
//        self.log.info("Life Ended: Result - %s" % str(result))
//        return result
    
//    def driver(self,
//               drive_collection = None,
//               timestamp = current_time()):
//        # The main loop that runs the agent
//        if drive_collection == None:
//            drive_collection = self.drive_collection

//        if drive_collection.goal.ready():
//            return "drive_won"

//        # Simple routine to test frequency
//        def time_test(element, timestamp):
//            if (element.frequency <= 0) or \
//               (element.frequency < (timestamp - element.last_fired)):
                
//                ##########################################################
//                # timestamp was just time, but this crashed (Sam, 14/2/5)
//                # after the change it didn't crash but the things which
//                # used it never fired!  So changed > to <
//                ##########################################################
//                self.log.debug("Timetest 1")
//#                print "Timetest 1"
//                return 1
//            else:
//                self.log.debug("Timetest 0")
//#                print "Timetest 0"
//                return 0

//        for element_list in drive_collection.elements:
//            for element in element_list:
//                self.log.debug( \
//                "Processing Drive Element - %s" % str(element.drive_name))
//                if time_test(element, timestamp) and element.ready():
//                    self.log.debug( \
//                       "Drive - %s is ready to run" % str(element.drive_name))
//                    if self.schedule.run_drive(element.drive_name):
//                        self.log.debug("Drive - %s has 1+ items on the sched." % \
//                                       str(element.drive_name))
//                        element.last_fired = timestamp
//                    else:
//                        self.log.debug("Drive - %s has no items on the sched." % \
//                                       str(element.drive_name))
//                        self.log.debug("Adding drive_root to the sched.")
//                        element.drive_root.send( \
//                            content = element.drive_root.make_content(),
//                            timestamp = timestamp)
//                        # print element.drive_root.make_content()
//                        # print vars(element.drive_root.make_content())
//                    return 1
//                else:
//                    # We should put some debug stuff here
//                    self.log.debug( \
//                        "Drive - %s inhibited or not ready" % \
//                        str(element.drive_name))
//#                    print element
                    
//        # If nothing happened, we have a problem
//        self.log.error("All drives lost focus - ended or will not run")
//        return "drive_lost"
