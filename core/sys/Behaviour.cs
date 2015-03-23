﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.strict;
using System.Text.RegularExpressions;
using POSH.sys;
using System.Reflection;
using POSH.sys.annotations;

namespace POSH.sys
{
    

   

    /// <summary>
    /// Behaviour base class.
    /// </summary>
    public class Behaviour : LogBase
    {
        public static readonly string ATTRIBUTES = "attributes", ACTIONS = "actions", PRIMITIVES = "primitives",
            SENSES="senses", INSPECTORS ="inspectors";

        protected internal AgentBase agent;
        internal Random random;

        /// <summary>
        /// This string contains all lap plan names which the behaviour is suited for. The plan names are separated by '|'. 
        /// If no plan names are entered and the suitedPlans is left empty the behaviour is suitable for all agents.
        /// </summary>
        public string suitedPlans { protected internal set; get; }

        //public List<string> actions{get; private set;}
        /// <summary>
        /// Returns a list of available senses.
        /// </summary>
        //public List<string> senses {get; private set;}
        //private List<string> inspectors;getN

        Behaviour(AgentBase agent)
            : base("Behaviour", agent)
        {
            this.agent = agent;
            this.suitedPlans = string.Empty;
            // aquire the random number generator from the agent
            this.random = agent.random;
            this.attributes = new Dictionary<string, object>();
        }

		public Behaviour(AgentBase agent,string [] actions,string []senses) 
			: this(agent,actions,senses,null, null)
		{}

        /// <summary>
        /// Initialises behaviour with given actions and senses.
        /// 
        /// The actions and senses has to correspond to
        ///   - the method names that implement those actions/senses
        ///   - the names used in the plan
        /// 
        /// The log domain of a behaviour is set to
        /// [AgentId].Behaviour
        /// </summary>
        /// <param name="agent">The agent that uses the behaviour</param>
        /// <param name="actions">The action names to register.</param>
        /// <param name="senses">The sense names to register.</param>
        /// <param name="attributes">List of attributes to initialise behaviour state.</param>
        /// <param name="caller"></param>
        public Behaviour(AgentBase agent,string [] actions,string []senses,Dictionary<string,object> a_attributes,Behaviour caller) 
            : this(agent)
        {

            Dictionary<string, SortedList<float,POSHPrimitive>> availableActions = new Dictionary<string, SortedList<float,POSHPrimitive>>();
            Dictionary<string, SortedList<float,POSHPrimitive>> availableSenses = new Dictionary<string, SortedList<float,POSHPrimitive>>();

            if (caller != null)
                GetActionsSenses(caller);
            else 
            {
                MethodInfo[] methods = this.GetType().GetMethods();

                availableActions = ExtractPrimitives(this, true);
                availableSenses = ExtractPrimitives(this,false);
                
                
                // filtering the primitives which sould not be made available for the agent
                FilterPrimitives(actions,availableActions);
                FilterPrimitives(senses,availableSenses);



                this.attributes[ACTIONS] = availableActions;
                this.attributes[SENSES] = availableSenses;
            }

            this.attributes.Add(INSPECTORS,null);

            // assign additional attributes
            if (a_attributes != null)
                AssignAttributes(a_attributes);

        }

        /// <summary>
        /// removes all entries of the dictionary which are mentioned in the filter
        /// As only the dict reference is passed in the dictionary will be modified even if it is not passed back.
        /// </summary>
        /// <param name="filter">array of primitiv names which should not be made available for the agent</param>
        /// <param name="dict">the primitives which are available from a specific behaviour</param>
        private void FilterPrimitives(string[] filter, Dictionary<string, SortedList<float, POSHPrimitive>> dict)
        {
            if (filter != null && filter.Length > 0)
                foreach (string key in filter)
                    if (dict.ContainsKey(key))
                        dict.Remove(key);
        }

        /// <summary>
        /// The method is extracting the primitives based on their string names from a behaviour object.
        /// At this point all possible actions/ senses are extracted and their names are returned as a string list.
        /// </summary>
        /// <param name="source">The behaviour to search for the primitives</param>
        /// <param name="acts">True if search for Actions,\n False if searching for Senses</param>
        /// <returns>Returns a Dict containing the plan name as key and the correct Attribute defintions 
        /// all for Actions/Senses inside the given behaviour linked to the specific plan name</returns>
        private Dictionary<string, SortedList<float, POSHPrimitive>> ExtractPrimitives(Behaviour source, bool acts)
        {
            MethodInfo[] methods = source.GetType().GetMethods();
            Dictionary<string, SortedList<float, POSHPrimitive>> primitives = new Dictionary<string, SortedList<float, POSHPrimitive>>();


            foreach (MethodInfo method in methods)
            {
                POSHPrimitive prim = null;

                // if we want to add actions
                if (acts)
                {
                    // include versioning of primitives at some later point 
                    if (method.GetCustomAttributes(typeof(ExecutableAction), true).Length > 0)
                        prim = method.GetCustomAttributes(typeof(ExecutableAction), true).First() as ExecutableAction;
                }
                // if we want to add senses
                else
                    if (method.GetCustomAttributes(typeof(ExecutableSense), true).Length > 0)
                        prim = method.GetCustomAttributes(typeof(ExecutableSense), true).First() as ExecutableSense;

                // if there is a primitive we want we store it in a sorted list using the version number as a criteria for later retrieval
                if (prim != null)
                {
                    // linking the primitive to the method which it augments
                    prim.SetLinkedMethod(method.Name);
                    prim.SetOriginatingBhaviour(this);

                    if (primitives.ContainsKey(prim.command))
                    {
                        primitives[prim.command].Add(prim.version, prim);
                    }
                    else
                    {
                        SortedList<float, POSHPrimitive> list = new SortedList<float, POSHPrimitive>();
                        list[prim.version] = prim;
                        primitives[prim.command] = list;
                    }


                }
            }
 
            return primitives;
        }

        /// <summary>
        /// @deprecated I just copied this method from the python code but is seems to overcomplicated stuff and using methods from other behaviours is hard to communicate to the user
        /// </summary>
        /// <param name="sourceObj"></param>
        private void GetActionsSenses(Behaviour sourceObj)
        {
           try
            {
                if (sourceObj.attributes is Dictionary<string, object>)
                {
                    // check if the source object contains more actions and add those to the current attribute dict
                    if (sourceObj.attributes.ContainsKey(ACTIONS) && attributes.ContainsKey(ACTIONS) && attributes[ACTIONS] is Dictionary<string, Behaviour>)
                    {
                        Dictionary<string, Behaviour> a = (Dictionary<string,Behaviour>)sourceObj.attributes[ACTIONS];
                        foreach (KeyValuePair<string, Behaviour> e in a)
                            ((Dictionary<string, Behaviour>)this.attributes[ACTIONS])[e.Key] = e.Value;
                           
                    }
                    // check if the source object contains more senses and add those
                    if (sourceObj.attributes.ContainsKey(SENSES) && attributes.ContainsKey(SENSES) && attributes[SENSES] is Dictionary<string, Behaviour>)
                    {
                        Dictionary<string, Behaviour> s = (Dictionary<string, Behaviour>)sourceObj.attributes[SENSES];
                        foreach (KeyValuePair<string, Behaviour> e in s)
                            ((Dictionary<string, Behaviour>)this.attributes[SENSES])[e.Key] = e.Value;
                            
                    }
                }
            }
            catch(ArgumentNullException ){}
            

        }

        public bool ExecuteAction(string actionMethod)
        {
            object result = ExecuteSense(actionMethod);

            return (result is bool) ? (bool)result : false;
        }
        
        public object ExecuteSense(string senseMethod)
        {
            object result = null;

            System.Reflection.MethodInfo methodInfo = this.GetType().GetMethod(senseMethod, new Type[] { });
            if (methodInfo is System.Reflection.MethodInfo && methodInfo.IsPublic)
                result = methodInfo.Invoke(this, new object[] { });
            return result;
        }
        
        /// <summary>
        /// Returns the name of the behaviour.
        ///
        /// The name of a behaviour is the same as the name of
        /// the class that implements it.
        /// </summary>
        public string GetName()
        {
            return this.GetType().FullName.ToString();
        }

        /// <summary>
        /// Allows To limit the freedom of the action selection by inhibiting agents the usage of certain behaviours.
        /// This follows the idea of Object orientation and Accessability of unneeded information.
        /// </summary>
        /// <param name="agent">The agentbase containing information regarding the used lap plan</param>
        /// <returns>True if the behaviour should be usable by the agent. False otherwise.</returns>
        public bool IsSuitedForAgent(AgentBase agent)
        {
            if (suitedPlans.Trim() == string.Empty)
                return true;
            if (!suitedPlans.Contains('|'))
                return ( agent.linkedPlanName == suitedPlans);
            string [] plans = suitedPlans.Split(new char[]{'|'});

            return (plans.Contains(agent.linkedPlanName));
        }

        public void SetSuitablePlans(string[] plans)
        {
            foreach (string plan in plans)
                suitedPlans += plan + "|";
        }

        /// <summary>
        /// Sets the random number generator of the behaviour.
        /// 
        /// This method is called whenever the random number generator
        /// of the agent is changed. The random number generator is
        /// accessible through the behaviour attribute 'random'.__abs__
        /// </summary>
        /// <param name="generator">A random number generator.</param>
        public void SetRNG(Random generator)
        {
            random=generator;
        }

        /// <summary>
        /// Assigns the behaviour a set of attributes.
        /// 
        /// The attributes are given by a dictionary attribute_name -> value.
        /// If the behaviour object already has an attribute with the given name,
        /// this attribute is only reassigned if it is not callable (e.g. a
        /// method) or a reserved attribute (see Behaviour._reserved_attributes
        /// for a list).
        /// </summary>
        /// <param name="attributes">dictionary of attributes to assign to behaviour.</param>
        public virtual void AssignAttributes(Dictionary<string,object> attribs)
        {

            foreach (KeyValuePair<string, object> e in attribs)
                AssignAttribute(e.Key,e.Value);
        }

        public virtual void AssignAttribute(string key, object attrib)
        {

            if (key != ACTIONS && key != SENSES && key != INSPECTORS)
                if (attrib.GetType().IsSubclassOf(typeof(POSH.sys.strict.POSHAction)))
                    ((Dictionary<string, object>)this.attributes[ACTIONS])[key] = attrib;
                else
                    if (attrib.GetType().IsSubclassOf(typeof(POSH.sys.strict.POSHSense)))
                        ((Dictionary<string, object>)this.attributes[SENSES])[key] = attrib;
                    else
                        this.attributes[key] = attrib;
        }            
                            
        /// <summary>
        /// Returns if the behaviour is ok.
        /// 
        /// This method is called to make sure that the behaviour is ok at every
        /// cycle. In its default implementation it always returns False.
        /// </summary>
        /// <returns>False for OK, True for not OK.</returns>
        public virtual bool  CheckError()
        {
            return false;
        }

        /// <summary>
        /// Called by the agent upon a request for exit.
        /// 
        /// This method prepares the behaviour to stop. In its default
        /// implementation it does nothing.
        /// </summary>
        public virtual void ExitPrepare()
        {
        }

        /// <summary>
        /// Called by the agent before the main loop is started.
        ///
        /// This is the best place to connect the behaviours to the world, if
        /// required, or register them there. If, for example, a behaviour
        /// needs to establish a network connection, then this is the best place
        /// to establish this connection.
        ///
        /// The method has to return if it was successful or not. Alternatively, it
        /// can always return True, and then report the behaviour's state in
        /// checkError() which is usualy called after all behaviours have been
        /// resetted.
        ///
        /// In its default implementation, this method returns True.
        /// </summary>
        /// <returns>If the reset was successful.</returns>
        public virtual bool Reset()
        {
            return true;
        }

        /// <summary>
        /// Sets the methods to call to get/modify the state of the behaviour.
        /// 
        /// Inspectors can be used to observe and modify the state of a behaviour.
        /// Each inspector has to be given by a string that gives the name of
        /// the behaviour method to be called. The method has to be named 'get'
        /// followed by the name of the inspector, and has to take 0 arguments
        /// (other than C{self}, naturally). If you want to allow changing
        /// the behaviour state, you have to provide another method taking a single
        /// string as an argument (besides the obligatory C{self}), and being
        /// called 'set' followed by the name of the inspector.
        /// 
        /// Given, for example, that we want to control the energy level of a
        /// behaviour. Then, if the string 'Energy' is given to the
        /// inspector, it looks for the method 'getEnergy' to get the energy level.
        /// If another method 'setEnergy' is provided, taking a string as an
        /// argument, we can also modify the energy level of the behaviour.
        /// 
       // @raise AttributeError: If the inspector method cannot be found.
        /// </summary>
        /// <param name="inspects">A list of inspector methods, as described above.</param>
        void RegisterInspector(string [] inspects)
        {
            Delegate accessor,mutator;
            if (!this.attributes.ContainsKey(INSPECTORS))
                this.attributes.Add(INSPECTORS,new Dictionary<string,Tuple<Delegate,Delegate>>());

            Dictionary<string,Tuple<Delegate,Delegate>> inspectors= new Dictionary<string,Tuple<Delegate,Delegate>>();
            //Regex r=new Regex("get%s");
            //Regex s=new Regex("set%s");

            foreach(string key in this.attributes.Keys)
            {
                accessor=this.attributes.ContainsKey("get"+key) ? (Delegate)this.attributes["get"+key] : null;
                mutator=this.attributes.ContainsKey("set"+key) ? (Delegate)this.attributes["set"+key] : null;

                if (accessor != null)
                {
                    inspectors.Add(key,new Tuple<Delegate,Delegate>(accessor,mutator));



                } else {
                    throw new NullReferenceException("Could not find inspector method "+accessor);
                    //raise AttributeError, "Could not find inspector method %s " \
                    //"in behaviour %s" % (inspector, self._name)
                }
            }
            this.attributes[INSPECTORS]=inspectors;
                
        }

        /// <summary>
        /// Returns the list of currently registered inspectors.
        /// 
        /// The list of inspectors contains elements of the form
        /// C{(name, accessor, mutator)}, where C{name} is the name of the
        /// inspector, C{accessor} is the accessor method (taking no arguments),
        /// and C{mutator} is the mutator method (taking a single string as its
        /// only argument), or C{None} if no mutator is provided.
        /// </summary>
        /// <returns>List of inspectors.</returns>
        Dictionary<string,Tuple<Delegate,Delegate>> GetInspectors()
        {
            return attributes.ContainsKey(INSPECTORS) ? (Dictionary<string,Tuple<Delegate,Delegate>>)attributes[INSPECTORS] : null;
        }

    

    }
}
