using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys.exceptions;
using POSH_sharp.sys.strict;

namespace POSH_sharp.sys
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
        Dictionary<string, Behaviour> _actions;
        Dictionary<string, Behaviour> _senses;
        /// <summary>
        /// Initialises the behaviour dictionary.
        /// </summary>
        public BehaviourDict()
        {
            _behaviours = new Dictionary<string, Behaviour>();
            _actions = new Dictionary<string, Behaviour>();
            _senses = new Dictionary<string, Behaviour>();

        }

        /// <summary>
        /// Registers the given behaviour.

        ///    Upon registering, it is checked if all action and sense
        ///    methods are actually available in the given behaviour.
        ///    If that is not the case, an AttributeError is thrown.

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
            Dictionary<string, POSHAction> actions = (Dictionary<string, POSHAction>)behave.attributes[Behaviour.ACTIONS];
            Dictionary<string, POSHSense> senses = (Dictionary<string, POSHSense>)behave.attributes[Behaviour.SENSES];
            string behaviourName = behave.GetName();

            //    # add the behaviour
            if (_behaviours.ContainsKey(behaviourName))
                throw new NameException(String.Format("Behaviour {0} cannot be registered twice", behaviourName));
            _behaviours.Add(behaviourName, behave);
            //    # add the actions
            foreach (KeyValuePair<string, POSHAction> action in actions)
            {
                if (this._actions.ContainsKey(action.Key))
                    throw new AttributeException(String.Format("Action {0} cannot be registered twice", action));
                this._actions.Add(action.Key, behave);
            }
            //    # .. and the senses
            foreach (KeyValuePair<string, POSHSense> sense in senses)
            {
                if (this._senses.ContainsKey(sense.Key))
                    throw new AttributeException(String.Format("Sense {0} cannot be registered twice", sense));
                this._senses.Add(sense.Key, behave);
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
        /// Returns an action by name.
        /// </summary>
        /// <param name="actionName">The name of a registered action.</param>
        /// <returns>The action with the given name. If no action with the specified name was registered null is returned.</returns>
        public Tuple<string,Behaviour> getAction(string actionName)
        {
            if (!_actions.ContainsKey(actionName))
                throw new NameException(string.Format("Action '{0}' not provided by any behaviour",
                    actionName));

            return new Tuple<string, Behaviour>(actionName, _actions[actionName]);
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

            return _actions[actionName];
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

            return _senses.ContainsKey(senseName) ? new Tuple<string,Behaviour>(senseName,_senses[senseName]) : null;
        }

        /// <summary>
        /// Returns a list of available sense names.
        /// </summary>
        /// <returns>A list of sense names.</returns>
        public string[] getSenseNames()
        {
            return _senses.Keys.ToArray();
        }

        /// <summary>
        /// Returns the behaviour that provides the given sense.
        /// </summary>
        /// <param name="senseName">The sense that the behaviour provides</param>
        /// <returns>The behaviour that provides the given sense.</returns>
        public Behaviour getSenseBehaviour(string senseName)
        {
            return _senses[senseName];
        }

    }

}

    
    
