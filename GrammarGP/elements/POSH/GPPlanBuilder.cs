using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys;
using POSH.sys.exceptions;
using GrammarGP.env;

namespace GrammarGP.elements.POSH
{
    public class GPPlanBuilder
    {
        private string[] docString;
        public Tuple<string, string, List<object>, List<Tuple<string, List<object>, string, long>[]>> driveCollection { private set; get; }
        public Dictionary<string, Tuple<string, long, List<object>>> actionPatterns { private set; get; }
        public Dictionary<string, Tuple<string, long, List<object>, List<Tuple<string, List<object>, string, int>[]>>> competences { private set; get; }
        public List<POSHAction> actions { private set; get; }
        public List<POSHSense> senses { private set; get; }


        /// <summary>
        /// Initialises the plan builder.
        /// </summary>
        public GPPlanBuilder()
        {
            // store drive collections, action pattern, competences, docstring
            docString = null;
            driveCollection = null;
            actionPatterns = new Dictionary<string,Tuple<string,long,List<object>>>();
            competences = new Dictionary<string,Tuple<string,long,List<object>,List<Tuple<string,List<object>,string,int>[]>>>();
            senses = new List<POSHSense>();
        }

        /// <summary>
        /// Sets the docstring of the plan.
        /// 
        /// This string is not used for plan generation.
        /// Calling this method replaces an alreay set docstring.
        /// </summary>
        /// <param name="docString">The docstring as list of strings.
        ///         [string, string, string]</param>
        public void setDocString(string[] docString)
        {
            this.docString = docString;
        }

        /// <summary>
        /// Sets the drive collection of the plan.
        /// 
        /// The drive collection has to be given in the following format:
        /// (type, name, goal, priorities)
        /// where:
        ///     - type: string, any of DC, RDC, SDC, SRDC
        ///     - name: string, name of drive collection
        ///     - goal: a goal list as described below
        ///     - priorities: a list of comptence priorities as described below
        ///
        /// A goal is a sequence of senses and sense-acts, where sense-acts
        /// are given by their name as a string, and senses are given by a
        /// triple of the form (name, value, predicate), where all elements
        /// are given as string. Valid values for predicates are discussed in
        /// the documentation of L{POSH.strict.Sense}. If there is no goal, then
        /// None can be given instead of the goal, which is treated equivalently
        /// to an empty list.
        ///
        /// A list of priorities is a sequence of collections of drive elements,
        /// like [[drive element 1a, drive element 1b], [drive element 2a], ...],
        /// Each drive element is a quadruple given by (name, trigger, triggerable,
        /// frequency), where the name is a string, the trigger is - just as a
        /// goal - a collection of senses and sense-acts, a triggerable is given
        /// by its name, and the frequency is a long integer. If there is no
        /// trigger (i.e. the element is always triggered), then None can be
        /// given as a trigger.
        /// </summary>
        /// <param name="collection"></param>
        public void SetDriveCollection(Tuple<string,string,List<object>,List<Tuple<string,List<object>,string,long>[]>> collection)
        {
            this.driveCollection = collection;
        }

        /// <summary>
        /// Adds the given action pattern to the plan.
        ///
        /// The given action pattern has to be a triple of the form (name,
        /// time, action squence), where the name is given by a string,
        /// the time is given as a long integer (or None, if no time is
        /// specified), and the action sequence is given as a sequence of
        /// strings that give the action / sense-act / competence names, or
        /// triples of the form (name, value, predicate), where all elements
        /// are given as string and the triple describes a sense. Valid values
        /// for predicates are discussed in the documentation of L{POSH.strict.Sense}.
        /// </summary>
        /// <param name="pattern">A structure describing the action pattern.</param>
        public void addActionPattern(Tuple<string,long,List<object>> pattern)
        {
            string name = pattern.First;
            if (actionPatterns.ContainsKey(name))
                throw new NameException(string.Format("More than one action pattern named '{0}'", name));
            else if (competences.ContainsKey(name))
                throw new NameException(string.Format("Action pattern name '{0}' "+
                    "clashes with competence of same name", name));
            actionPatterns[name] = pattern;
        }
    
        /// <summary>
        /// Adds the given competence to the plan.
        /// 
        /// The competence structure is similar to a drive collection and
        /// is described by the quatruple (name, time, goal, priorities).
        /// The name is given as string, and the time as a long integer (or None
        /// if no time is specified).
        /// 
        /// A goal is a sequence of senses and sense-acts, where sense-acts
        /// are given by their name as a string, and senses are given by a
        /// triple of the form (name, value, predicate), where all elements
        /// are given as string. Valid values for predicates are discussed in
        /// the documentation of L{POSH.strict.Sense}. If either vqlue or predicate are
        /// not specified, then None can be given instead of the string.
        /// If there is no goal, then None can be given instead of the goal,
        /// which is treated equivalently to an empty list.

        /// A list of priorities is a sequence of collections of competence
        /// elements, like [[competence element 1a, competence element 1b],
        /// [competence element 2a], ...]. Each competence element is a quadruple
        /// given by (name, trigger, triggerable, retries), where the name is
        /// a string, the trigger is - just as a goal - a collection of senses
        /// and sense-acts, a triggerable is given by its name, and the number of
        /// retries is is a long integer. If there is no trigger (i.e. the element
        /// is always triggered), then None can be  given as a trigger.
        /// </summary>
        /// <param name="competence">A structure describing a competence.</param>
        /// <exception cref="NameException">If there is already an action pattern or competence
        ///     with the same name in the plan.</exception>
        public  void addCompetence(Tuple<string,long,List<object>,List<Tuple<string,List<object>,string,int>[]>> competence)
        {
 	        string name = competence.First;

            if (competences.ContainsKey(name))
                throw new NameException(string.Format("More than one competence named '{0}'", name));
            else if (actionPatterns.ContainsKey(name))
                throw new NameException(string.Format("Competence name '{0}' "+
                    "clashes with action pattern of same name", name));
            competences[name] = competence;
        }

        /// <summary>
        /// Builds the plan and returns the drive collection.
        /// 
        /// This method operates in several stages:
        /// 
        ///  1. It is checked if none of the action pattern or competence
        ///     names are already taken by an action or sense/sense-act
        ///     in the behaviour library. If a conflict
        ///     is found, then NameError is raised.
        /// 
        ///  2. All competence / action pattern objects are created, together
        ///     with goals and triggers, but their elements are left empty.
        /// 
        ///  3. The elements of competences and action pattern are created.
        /// 
        ///  4. The drive collection is built and returned.
        /// </summary>
        /// <param name="agent">The agent that uses the plan.</param>
        /// <returns>The drive collection as the root of the plan.</returns>
        /// <exception cref="NameException">
        /// If clashes in naming of actions / action pattern /
        ///    competences were found, or if a sense / action / sense-act was
        ///    not found.
        /// </exception>
        public DriveCollection Build(IChromosome chrom, Configuration config)
        {
            Dictionary<string,Competence> competences= BuildCompetenceStubs(chrom, config);
            Dictionary<string, ActionPattern> actionPatterns = BuildActionPatternStubs(chrom, config);
            BuildCompetences(chrom, config, competences, actionPatterns);
            buildActionPatterns(chrom, config, competences, actionPatterns);

            return BuildDriveCollection(chrom, config, competences, actionPatterns);
        }

        

        /// <summary>
        /// Builds the drive collection and returns it.
        /// 
        /// This method builds the drive collection, of which the structure has
        /// been set by setDriveCollection. Additionally, its
        /// assigns the agent a timer, as specified by the drive collection
        /// (i.e. a stepped timer, in the case of an SDC drive, and a
        /// real-time timer in the case of an SRDC drive). If the timer is
        /// a real-time timer, then it is initialised with a loop frequency of
        /// 50Hz.
        /// 
        /// Only drives of type 'SDC' and 'SRDC' are accepted. In any other case
        /// a TypeError is raised.
        /// </summary>
        /// <param name="agent">The agent that the drive collection is built for.</param>
        /// <param name="competences">A competence object dictionary.</param>
        /// <param name="actionPatterns">An action pattern dictionary.</param>
        /// <exception cref="TypeLoadException">For drives of types other than SDC or SRDC.</exception>
        /// <returns>The drive collection.</returns>
        internal DriveCollection BuildDriveCollection(IChromosome chrom, Configuration config, Dictionary<string,Competence> competences, Dictionary<string,ActionPattern> actionPatterns)
        {
            string dcType = driveCollection.First;
            string dcName = driveCollection.Second;
            List<DrivePriorityElement> priorityElements = new List<DrivePriorityElement>();

            // create the agent timer
            switch (dcType)
            {
                case "SDC":
                    break;
                case "SRDC":
                    break;
                default:
                    throw new TypeLoadException(string.Format("Drive collection of type '{0}' not " +
                        "supported (only supporting SDC and SRDC).", dcType));
            }
            Goal goal = BuildGoal(chrom, config, driveCollection.Third);
            
            foreach ( Tuple<string,List<object>,string,long>[] priorityElement in driveCollection.Forth)
            {
                List<DriveElement> elementList = new List<DriveElement>();

                foreach(Tuple<string,List<object>,string,long> element in priorityElement)
                {
                    DriveElement driveElement = BuildDriveElement(chrom, config, element, competences, actionPatterns);

                    elementList.Add(driveElement);
                }

                DrivePriorityElement dpe = new DrivePriorityElement(chrom, config, dcName);
                dpe.SetChildren(elementList.ToArray());
                priorityElements.Add(dpe);
            }
            DriveCollection dc = new DriveCollection(chrom, config, dcName, dcType);
            
            List<AGene> dcChildren = new List<AGene>(priorityElements.ToArray());
            dcChildren.Add(goal);
            dc.SetChildren(dcChildren.ToArray());
            return dc;
        }

        /// <summary>
        /// Builds a drive element according to the given structure.
        /// 
        /// The structure of a drive element is described in
        /// setDriveCollection.
        /// </summary>
        /// <param name="element">The structure of the drive element to build.</param>
        /// <param name="agent">The agent to build the drive collection for.</param>
        /// <param name="competences">A competences object dictionary.</param>
        /// <param name="actionPatterns">An action pattern object dictionary.</param>
        /// <returns>A drive element. <code>POSH.strict.DriveElement</code></returns>
        internal DriveElement BuildDriveElement(IChromosome chrom, Configuration config, Tuple<string, List<object>, string, long> element,
            Dictionary<string,Competence> competences, 
            Dictionary<string,ActionPattern> actionPatterns)
        {
            Goal trigger = BuildTrigger(chrom, config, element.Second);
            AGene triggerAble = GetTriggerable(chrom, config, element.Third, competences, actionPatterns);
            DriveElement de = new DriveElement(chrom, config, element.First);
            de.SetChildren(new AGene[]{trigger,triggerAble,new Terminal(chrom, config,AGene.ReturnType.Number , element.Forth)});
            
            return de;
        }

        /// <summary>
        /// Completes the competences based on the given competence stubs.
        /// 
        /// This method modifies the given competence stubs and creates
        /// all its elements. These elements can either be other
        /// competences, action pattern, or actions. In case of actions,
        /// the corresponding POSH.strict.Action objects are created. The
        /// actions have to be available in the agent's behaviour dictionary.
        /// </summary>
        /// <param name="agent">The agent to build the competences for.</param>
        /// <param name="competences">The competence stubs, as returned by
        ///     buildCompetenceStubs</param>
        /// <param name="actionPatterns">The action pattern stubs, as returned by 
        ///     buildActionPatternStubs</param>
        internal void BuildCompetences(IChromosome chrom, Configuration config, Dictionary<string,Competence> competences, 
            Dictionary<string,ActionPattern> actionPatterns)
        {
            foreach (string competence in competences.Keys)
            {
                List<CompetencePriorityElement> priorityElements = new List<CompetencePriorityElement>();
                // start with priority elements
                foreach ( Tuple<string,List<object>,string,int>[] priorityElement in this.competences[competence].Forth)
                {
                    List<CompetenceElement> elementList = new List<CompetenceElement>();
                    foreach (Tuple<string,List<object>,string,int> element in priorityElement)
                        elementList.Add(BuildCompetenceElement(chrom, config, element, competences, actionPatterns));
                    CompetencePriorityElement ce = new CompetencePriorityElement(chrom, config,competence);
                    ce.SetChildren(elementList.ToArray());
                    priorityElements.Add(ce);
                }
                competences[competence].SetChildren(priorityElements.ToArray());
            }
        }

        /// <summary>
        /// Completes the action pattern based on the given
        /// action pattern stubs.
        /// 
        /// This method modifies the given action pattern stubs and
        /// creates all its elements. The elements can either be other
        /// competences, actions or senses, where competences are only
        /// allowed as the last elements in action patterns. In case of
        /// actions / senses, the corresponding POSH.strict.Action /
        /// POSH.strict.Sense objects are created. The actions / senses have
        /// to be available in the agent's behaviour dictionary.
        /// </summary>
        /// <param name="agent">The agent to build the competences for.</param>
        /// <param name="competences">The competence stubs, as returned by 
        ///     buildCompetenceStubs.</param>
        /// <param name="actionPatterns">The action pattern stubs, as returned by 
        ///     buildActionPatternStubs</param>
        /// <returns></returns>
        internal void buildActionPatterns(IChromosome chrom, Configuration config, Dictionary<string, Competence> competences, 
            Dictionary<string,ActionPattern> actionPatterns)
        {
            List<string> senseNames = new List<string>();
            
            foreach (AGene sense in config.genePool.GetAllGenes(AGene.GeneType.Sense))
                senseNames.Add((string)sense.value);
            
            foreach (string actionPattern in this.actionPatterns.Keys)
            {
                // create the elements of the action pattern
                List<AGene> elementList = new List<AGene>();
                object[] elementNames = this.actionPatterns[actionPattern].Third.ToArray();
                
                // build all but the last element
                for (int i = 0; i < elementNames.Length - 1; i++ )
                {
                    if (elementNames[i] is string && senseNames.Contains((string)elementNames[i]))
                        //its in senses and a string -> sense-act
                        elementList.Add(BuildSenseAct(chrom, config, (string)elementNames[i]));
                    else if (elementNames[i] is Tuple<string,string,string>)
                        //its not a string -> sense
                        elementList.Add(BuildSense(chrom, config, (Tuple<string, string, string>)elementNames[i]));
                    else 
                        //neither of above -> action
                        elementList.Add(GetTriggerable(chrom, config, (string)elementNames[i]));
                }
                // for the last element also allow competences
                if (elementNames[elementNames.Length-1] is string && senseNames.Contains((string)elementNames[elementNames.Length-1]))
                    //its in senses and a string -> sense-act
                    elementList.Add(BuildSenseAct(chrom, config, (string)elementNames[elementNames.Length - 1]));
                else if (elementNames[elementNames.Length-1] is Tuple<string,string,string>)
                    //its not a string -> sense
                    elementList.Add(BuildSense(chrom, config, (Tuple<string, string, string>)elementNames[elementNames.Length - 1]));
                else 
                    //neither of above -> action
                    elementList.Add(GetTriggerable(chrom, config, (string)elementNames[elementNames.Length-1],competences));
                
                actionPatterns[actionPattern].SetChildren(elementList.ToArray());
            }
        }

        /// <summary>
        /// Builds stub objects for the plan competences.
        /// 
        /// The stub competences are competences without elements.
        /// </summary>
        /// <param name="agent">The agent to build the competences for.</param>
        /// <returns>A dictionary with competence stubs.</returns>
        internal Dictionary<string, Competence> BuildCompetenceStubs(IChromosome chrom, Configuration config)
        {
            Dictionary<string,Competence> competenceStubs = new Dictionary<string,Competence>();

            foreach(KeyValuePair<string, Tuple<string, long, List<object>, List<Tuple<string,List<object>,string,int>[]>>> pair 
                in this.competences)
            {
                Goal goal = BuildGoal(chrom,config,pair.Value.Third);
                // FIXME: @swen: this is an actual issue as the plan allows setting a timeout which the engine currently is ignoring
                Competence c = new Competence(chrom, config, pair.Key);
                c.SetChildren(new AGene[]{goal});
                competenceStubs[pair.Key] = c;
            }

            return competenceStubs;
        }
        
        /// <summary>
        /// Build stub objects for the plan action pattern.
        /// 
        /// The stub action pattern are action pattern without actions.
        /// </summary>
        /// <param name="agent"> The action to build the action pattern for.</param>
        /// <returns>A dictionary with action pattern stubs.</returns>
        internal Dictionary<string, ActionPattern> BuildActionPatternStubs(IChromosome chrom, Configuration config)
        {
            Dictionary<string,ActionPattern> patternStubs = new Dictionary<string,ActionPattern>();
            
            foreach (KeyValuePair<string,Tuple<string,long,List<object>>> pair in this.actionPatterns )
                // INFO: OLD: we're just ignoring the time, as we use the simple slip-stack
                // FIXME: @swen: this is an actual issue as the plan allows setting a timeout which the engine currently is ignoring
                patternStubs[pair.Key] = new ActionPattern(chrom,config,pair.Key);
            
            return patternStubs;
        }
        
        /// <summary>
        /// Builds a competence element from the given structure.
        /// 
        /// The competence element has to be given as a the quadruple (name,
        /// trigger, triggerable, retries), where the name is a string, the
        /// trigger is described in L{addCompetence}, the triggerable is the
        /// name of an action, competence or action pattern, and retries is the
        /// number of retries and given as long.
        /// 
        /// If the triggerable cannot be found, then a NameError is raised.
        /// </summary>
        /// <param name="element">The structure of the competence element (Tuple[name,
        /// trigger, triggerable, retries]).</param>
        /// <param name="agent">The agent that the competence element is built for.</param>
        /// <param name="competences">A competence object dictionary.</param>
        /// <param name="actionPatterns">An action pattern object dictionary.</param>
        /// <returns>The competence element described by the given structure.</returns>
        /// <exception cref="NameException"> If the triggerable cannot be found.
        /// </exception>
        internal CompetenceElement BuildCompetenceElement(IChromosome chrom, Configuration config, Tuple<string, List<object>, string, int> element,
            Dictionary<string,Competence> competences, Dictionary<string,ActionPattern> actionPatterns)
        {
            CompetenceElement ce = new CompetenceElement(chrom, config, element.First);
            Goal trigger = BuildTrigger(chrom, config, element.Second);
            AGene triggerable = GetTriggerable(chrom, config, element.Third,competences,actionPatterns);
            ce.SetChildren(new AGene[]{trigger,triggerable,new Terminal(chrom,config,AGene.ReturnType.Number,element.Forth)});
            return ce;
        }

        
        /// <summary>
        /// Builds the trigger forn the given structure.
        /// 
        /// The given trigger structure is a sequence of sense-acts given
        /// by simple strings, and senses given by the triple (name, value,
        /// predicate), where all of the elements of the triple are string.
        /// Optionally, the value and predicate can be None.
        /// 
        /// If the trigger list is empty, or None is given for the trigger,
        /// then None is returned.
        /// </summary>
        /// <param name="agent">The agent to build the trigger for.</param>
        /// <param name="trigger">The sequence of senses and sense-acts: 
        ///     senses (<code>string</code>) and full-senses 
        ///     (<code>Tuple[string,string,string]</code>) </param>
        /// <returns> The trigger object</returns>
        internal Goal BuildTrigger(IChromosome chrom, Configuration config, List<object> trigger)
        {
            // is the same as buildGoal
            return BuildGoal(chrom, config, trigger);
        }

        /// <summary>
        /// Builds the goal from the given structure.
        /// 
        /// The given goal structure is a sequence of sense-acts given by
        /// simple strings, and senses given by the triple (name, value,
        /// predicate), where all of the elements of the triple are string.
        /// Optionally, the value and predicate can be None.
        /// 
        /// If the goal list is empty, or None is given for the goal,
        /// then None is returned.
        /// </summary>
        /// <param name="agent">The agent to build the goal for.</param>
        /// <param name="goalList">The sequence of senses and sense-acts: 
        ///     senses (string) and full-senses 
        ///     (Tuple[string,string,string]) </param>
        /// <returns>The goal object.</returns>
        internal Goal BuildGoal(IChromosome chrom, Configuration config, List<object> goalList)
        {
            List<POSHSense> senses = new List<POSHSense>();
            Goal goal = new Goal(chrom, config);
            if (!(goalList is List<object>) || goalList.Count == 0)
                return null;
            foreach(object sense in goalList)
                if (sense is string)
                    senses.Add(BuildSenseAct(chrom,config,(string)sense));
                else if (sense is Tuple<string,string,string>)
                    senses.Add(BuildSense(chrom,config,(Tuple<string,string,string>)sense));
            goal.SetChildren(senses.ToArray());
           
            return goal;
        }

        /// <summary>
        /// Returns a sense-act object for the given name.
        /// </summary>
        /// <param name="agent">The agent to build the sense-act for.</param>
        /// <param name="senseName">The name of the sense-act.</param>
        /// <returns>The created sense-act object</returns>
        internal POSHSense BuildSenseAct(IChromosome chrom, Configuration config, string senseName)
        {
            POSHSense sense = new POSHSense(chrom, config, senseName);
           
            if (!senses.Contains(sense))
                senses.Add(sense);
            return sense;
        }

        /// <summary>
        /// Returns a sense object for the given structure.
        /// 
        /// The structure is of the form (name, value, predicate), where
        /// the first is a string, and the other two elements are either
        /// a string or None.
        /// </summary>
        /// <param name="agent">The agent to build the sense for.</param>
        /// <param name="senseStruct">The sense structure. (see LAPParser.getFullSenses)</param>
        /// <returns>The created sense object</returns>
        /// <exception cref="NameException">
        /// If the sense could not be found in the behaviour dictionary.
        /// </exception>
        internal POSHSense BuildSense(IChromosome chrom, Configuration config, Tuple<string, string, string> senseStruct)
        {
            POSHSense sense = new POSHSense(chrom, config, senseStruct.First);
            object value = (senseStruct.Second is string) ? AgentInitParser.strToValue(senseStruct.Second) : null;

            Terminal term;
            if (value is string)
                term = new Terminal(chrom, config,AGene.ReturnType.Text, value);
            else if (value is bool)
                term = new Terminal(chrom, config, AGene.ReturnType.Bool, value);
            else if (value != null)
                term = new Terminal(chrom, config, AGene.ReturnType.Number, value);
            else 
                term = new Terminal(chrom, config, AGene.ReturnType.Bool, true);
            sense.SetChildren(new AGene[] { term, new Predicate(chrom, config, senseStruct.Third) });

            if (!senses.Contains(sense))
                senses.Add(sense);
            return sense;
        }

        internal AGene GetTriggerable(IChromosome chrom, Configuration config, string name)
		{
			return GetTriggerable ( chrom, config, name, null, null);
		}
        internal AGene GetTriggerable(IChromosome chrom, Configuration config, string name, Dictionary<string, Competence> competences)
		{
			return GetTriggerable ( chrom, config, name, competences, null);
		}

        /// <summary>
        /// Returns the action / competence / actionpattern with the given name.
        /// 
        /// This method looks for the element with the given name and
        /// returns it.  If the element is an action, then it creates a
        /// new POSHAction object from the agent's behaviour
        /// dictionary. Otherwise it just returns the competence or action
        /// pattern object.
        /// 
        /// The method also checks if the the given name is both an action and a
        /// competence / action pattern. In that case a NameError is raised.
        /// </summary>
        /// <param name="agent">The agent that the element belongs to.</param>
        /// <param name="name">The name of the element.</param>
        /// <param name="competences">A competence object dictionary.</param>
        /// <param name="actionPatterns">An action pattern object dictionary.</param>
        /// <returns>The element with the given name either POSHAction, Competence 
        ///     or ActionPattern.</returns>
        /// <exception cref="NameException">
        ///     If actions and competences / action pattern have
        ///     the same name.</exception>
        internal AGene GetTriggerable(IChromosome chrom, Configuration config, string name, Dictionary<string, Competence> competences, 
            Dictionary<string,ActionPattern> actionPatterns)
        {
            // creating an action would raise a NameError when looking up the
            // according behaviour in the behaviour dictionary. Hence, if no
            // behaviour provides that action, we need to check competences and
            // action pattern
            POSHAction element;

            try
            {
                element = new POSHAction(chrom, config, name);
                if (!actions.Contains(element))
                    actions.Add(element);
            } 
            catch (NameException)
            {
                if (competences is Dictionary<string,Competence> &&
                    competences.ContainsKey(name))
                    return competences[name];
                else if (actionPatterns is Dictionary<string,ActionPattern> &&
                    actionPatterns.ContainsKey(name))
                    return actionPatterns[name];
                else
                    throw new NameException(string.Format("No action / competence / action pattern " +
                      "with name '{0}' found", name));
            }
            // we get here only if the action was created successfully,
            // check now for clashes with competences / action pattern

            if ( (competences is Dictionary<string,Competence> && competences.ContainsKey(name) ) || 
                ( actionPatterns is Dictionary<string,ActionPattern> && actionPatterns.ContainsKey(name)))
                throw new NameException(string.Format("Name of action '{0}' also held by other " +
                "competence / action pattern", name));

            return element;
        }
    }
}
