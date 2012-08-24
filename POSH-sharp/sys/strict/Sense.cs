﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys.strict
{
//      _intMatcher = re.compile(r'^(0|\-?[1-9]\d*|0[0-7]+|0[xX][0-9a-fA-F]+)[lL]?$')
//      _floatMatcher = re.compile(r'^\-?(\d*\.\d+|\d+\.)([eE][\+\-]?\d+)?$')
//      _boolMatcher = re.compile(r'^[Tt]rue|[Ff]alse$')



    /// <summary>
    /// A sense / sense-act as a thin wrapper around a behaviour's
    /// sense / sense-act method.
    /// </summary>
    class Sense : CopiableElement
    {
        BehaviourDict behaviourDict;
        private Tuple<string,Behaviour> sense;
        protected internal Behaviour behaviour;
        private object value;
        string predicate;

        /// <summary>
        /// Picks the given sense or sense-act from the given agent.
        ///
        /// The method uses the agent's behaviour dictionary to get the
        /// sense / sense-act method.
        ///
        /// The log domain is set to "[AgentId].Sense.[sense_name]".
        ///
        /// The sense name is set to "[BehaviourName].[sense_name]".
        /// </summary>
        /// <param name="agent">The agent that can use the sense.</param>
        /// <param name="senseName">The name of the sense</param>
        /// <param name="value">The value to compare it to. This is given as a string,
        /// but will be converted to an integer or float or boolean, or
        /// left as a string, whatever is possible. If None is given
        /// then the sense has to evaluate to True.</param>
        /// <param name="predicate">"==", "!=", "<", ">", "<=", ">=". If null is
        ///    given, then "==" is assumed.</param>
        public Sense(Agent agent, string senseName, string value = null, string predicate = null)
            :base(string.Format("Sense.{0}",senseName),agent)
        {
            behaviourDict = agent.getBehaviourDict();
            sense = behaviourDict.getSense(senseName);
            behaviour = behaviourDict.getSenseBehaviour(senseName);
            name = string.Format("{0}.{1}", behaviour.getName(), senseName);
            this.value = (value is string) ? AgentInitParser.strToValue(value) : null;
            this.predicate = (predicate is string) ? predicate : "==";

            log.Debug("Created");
        }

        /// <summary>
        /// Activates the sense and returns its result.
        /// </summary>
        /// <returns>The result of the sense.</returns>
        public bool fire()
        {
            object result;
            log.Debug("Firing");

            result = sense.Second.executeSense(sense.First);


            if (value == null)
                return (bool) result;
            else if (predicate.Trim() == "==")
                return result == value;
            else if (predicate.Trim() == "!=")
                return result != value;
            else if (predicate.Trim() == "<=")
                return (float)result <= (float)value;
            else if (predicate.Trim() == ">=")
                return (float)result >= (float)value;
            else if (predicate.Trim() == "<")
                return (float)result < (float)value;
            else if (predicate.Trim() == ">")
                return (float)result > (float)value;
            else
                return (bool) result;
        }

        /// <summary>
        /// Returns itsself.
        /// 
        /// This method does NOT return a copy of the action as the action
        /// does not have an internal state and therefore doesn't need to
        /// be copied.
        /// </summary>
        /// <returns></returns>
        public override CopiableElement copy()
        {
            return this;
        }
    }
}