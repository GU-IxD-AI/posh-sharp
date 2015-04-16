using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.exceptions;
using POSH.sys.strict;
using POSH.sys.annotations;

namespace POSH.sys
{
    /// <summary>
    /// The behaviour dictionary.
    ///
    /// The behaviour dictionary is a dictionary of behaviours, its
    /// actions and senses. Each of the behaviours is registered
    /// with the dictionary. Subsequently, it allows looking up actions
    /// and senses by their names, and returns their behaviour and
    /// their actual method.
    /// </summary>
    public class BehaviourDict
    {
        Dictionary<string, Behaviour> _behaviours;
        Dictionary<string, SortedList<float,POSHPrimitive>> _actions;
        Dictionary<string, SortedList<float, POSHPrimitive>> _senses;
        /// <summary>
        /// Initialises the behaviour dictionary.
        /// </summary>
        public BehaviourDict()
        {
            _behaviours = new Dictionary<string, Behaviour>();

            _actions = new Dictionary<string, SortedList<float, POSHPrimitive>>();
            _senses = new Dictionary<string, SortedList<float, POSHPrimitive>>();

        }

        /// <summary>
        /// Registers the given behaviour.

        ///    Upon registering, it is checked if all action and sense
        ///    methods are actually available in the given behaviour.
        ///    If that is not the case, an AttributeException is thrown.

        ///    If there is already an action or a sense with the same
        ///    name registered in the behaviour dictionary, a
        ///    NameError is thrown.

        ///    The actions and senses are aquired by using the behaviour's
        ///    L{POSH.Behaviour.getActions} and L{POSH.Behaviour.getSenses}
        ///    methods.
        /// </summary>
        /// <param name="behave">The behaviour to register.</param>
        public void RegisterBehaviour(Behaviour behave)
        {
            Dictionary<string, SortedList<float, POSHPrimitive>> a_actions = (Dictionary<string, SortedList<float, POSHPrimitive>>)behave.attributes[Behaviour.ACTIONS];
            Dictionary<string, SortedList<float, POSHPrimitive>> a_senses = (Dictionary<string, SortedList<float, POSHPrimitive>>)behave.attributes[Behaviour.SENSES];
            string behaviourName = behave.GetName();

            //    # add the behaviour
            if (_behaviours.ContainsKey(behaviourName))
                throw new NameException(String.Format("Behaviour {0} cannot be registered twice", behaviourName));
            _behaviours.Add(behaviourName, behave);
            
            // add the actions
            AddPrimitives(_actions, (Dictionary<string, SortedList<float, POSHPrimitive>>)behave.attributes[Behaviour.ACTIONS], "Action {0} vs. {1} cannot be registered twice");

            // add the senses
            AddPrimitives(_senses, (Dictionary<string, SortedList<float, POSHPrimitive>>)behave.attributes[Behaviour.SENSES], "Sense {0} vs. {1} cannot be registered twice");

        }

        private void AddPrimitives(Dictionary<string, SortedList<float, POSHPrimitive>> target, Dictionary<string, SortedList<float, POSHPrimitive>> source, string exceptionText)
        {
            foreach (KeyValuePair<string, SortedList<float, POSHPrimitive>> prim in source)
            {
                if (target.ContainsKey(prim.Key))
                {
                    foreach (float version in source[prim.Key].Keys)
                        if (target[prim.Key].ContainsKey(version))
                            throw new AttributeException(String.Format(exceptionText, prim.Key, version));
                        else
                            target[prim.Key][version] = source[prim.Key][version];
                }
                else
                {
                    target[prim.Key] = prim.Value;
                }
            }
        }

        /// <summary>
        /// Returns a list of behaviours.
        /// </summary>
        /// <returns></returns>
        public Behaviour[] getBehaviours()
        {

            return _behaviours.Values.ToArray();
        }

        /// <summary>
        /// Returns the behaviour object with the given name.
        /// </summary>
        /// <param name="behaveName">The name of the behaviour.</param>
        /// <returns>Return the Behaviour with the given name. 
        /// If no behaviour with the given name was registered null is returned.</returns>
        public Behaviour getBehaviour(string behaveName)
        {
            if (!_behaviours.ContainsKey(behaveName))
                if (!behaveName.Contains("."))
                    foreach (string behave in _behaviours.Keys)
                    {
                        string fullname = behave.Split('.').Last(); 
                        if (fullname.Equals(behaveName))
                        {
                            behaveName = behave;
                            break;
                        }
                    }
                else
                    throw new NameException(string.Format("Cannot find Behaviour '{0}'",
                    behaveName));

            return _behaviours.ContainsKey(behaveName) ? _behaviours[behaveName] : null;
        }

        /// <summary>
        /// Returns an action by name and the linked behaviour.
        /// Important: The name return is the correct unique method name inside a specific behaviour. Also The highest version of the method is returned.
        /// </summary>
        /// <param name="actionName">The name of a registered action as used in the plan.</param>
        /// <returns>The method which  implments a given action name. If no action with the specified name was registered null is returned.</returns>
        public Tuple<string,Behaviour> getAction(string actionName)
        {
            if (!_actions.ContainsKey(actionName))
                throw new NameException(string.Format("Action '{0}' not provided by any behaviour",
                    actionName));

            return new Tuple<string, Behaviour>(_actions[actionName].Last().Value.linkedMethod, _actions[actionName].Last().Value.orginatingBehaviour);
        }

        /// <summary>
        /// Returns the list of available action names.
        /// </summary>
        /// <returns></returns>
        public string[] getActionNames()
        {
            return _actions.Keys.ToArray();
        }

        /// <summary>
        /// Returns the behaviour that provides the given action.
        /// </summary>
        /// <param name="actionName">The action that the behaviour provides</param>
        /// <returns>The behaviour that provides the given action.</returns>
        public Behaviour getActionBehaviour(string actionName)
        {
            if (!_actions.ContainsKey(actionName))
                throw new NameException(string.Format("Action '{0}' not provided by any behaviour",
                    actionName));

            return _actions[actionName].Last().Value.orginatingBehaviour;
        }

        /// <summary>
        /// Returns the behaviour that provides the given sense.
        /// </summary>
        /// <param name="senseName">The sense that the behaviour provides</param>
        /// <returns>The behaviour that provides the given sense.</returns>
        public Behaviour getSenseBehaviour(string senseName)
        {
            if (!_senses.ContainsKey(senseName))
                throw new NameException(string.Format("Sense '{0}' not provided by any behaviour",
                    senseName));

            return _senses[senseName].Last().Value.orginatingBehaviour;
        }

        /// <summary>
        /// Returns a sense by name.
        /// </summary>
        /// <param name="senseName">The name of a registered Sense.</param>
        /// <returns>The sense with the given name.</returns>
        public Tuple<string,Behaviour> getSense(string senseName)
        {
            if (!_senses.ContainsKey(senseName))
                throw new NameException(string.Format("Sense '{0}' not provided by any behaviour",
                    senseName));

            return _senses.ContainsKey(senseName) ? new Tuple<string, Behaviour>(_senses[senseName].Last().Value.linkedMethod, _senses[senseName].Last().Value.orginatingBehaviour) : null;
        }

        /// <summary>
        /// Returns a list of available sense names.
        /// </summary>
        /// <returns>A list of sense names.</returns>
        public string[] getSenseNames()
        {
            return _senses.Keys.ToArray();
        }

        

    }

}

    
    
