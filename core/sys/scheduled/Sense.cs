using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys.scheduled
{
//      _intMatcher = re.compile(r'^(0|\-?[1-9]\d*|0[0-7]+|0[xX][0-9a-fA-F]+)[lL]?$')
//      _floatMatcher = re.compile(r'^\-?(\d*\.\d+|\d+\.)([eE][\+\-]?\d+)?$')
//      _boolMatcher = re.compile(r'^[Tt]rue|[Ff]alse$')



    /// <summary>
    /// A sense / sense-act as a thin wrapper around a behaviour's
    /// sense / sense-act method.
    /// </summary>
    public class POSHSense : CopiableElement
    {
        BehaviourDict behaviourDict;
        private Tuple<string,Behaviour> sense;
        protected internal Behaviour behaviour;
        private object value;
        string predicate;

		public POSHSense(Agent agent, string senseName)
			:this(agent, senseName, null, null)
		{}

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
        public POSHSense(Agent agent, string senseName, string value, string predicate)
            :base(string.Format("Sense.{0}",senseName),agent)
        {
            behaviourDict = agent.getBehaviourDict();
            sense = behaviourDict.getSense(senseName);
            behaviour = behaviourDict.getSenseBehaviour(senseName);
            name = string.Format("{0}.{1}", behaviour.GetName(), senseName);
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

            result = sense.Second.ExecuteSense(sense.First);


            if (value == null)
                return (bool) result;
            else if (predicate.Trim() == "==")
                return result == value;
            else if (predicate.Trim() == "!=")
                return result != value;
            else if (predicate.Trim() == "<=")
            {
                if (result.GetType() == typeof(long))
                    return (long)result <= (long)value;
                return (float)result <= (float)value;
            }
            else if (predicate.Trim() == ">=")
            {
                if (result.GetType() == typeof(long))
                    return (long)result >= (long)value;
                return (float)result <= (float)value;
            }
            else if (predicate.Trim() == "<")
            {
                if (result.GetType() == typeof(long))
                    return (long)result < long.Parse(value.ToString());
                return (float)result <= (float)value;
            }
            else if (predicate.Trim() == ">")
            {
                if (result.GetType() == typeof(long))
                    return (long)result > long.Parse(value.ToString());
                return (float)result <= (float)value;
            }
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
//from __future__ import nested_scopes

//#import basic
//#import generic
//#import complex
//#import action_pattern
//#import blackboard
//#import competence
//#import competence_element
//#import drive_collection
//#import drive_element
//#import messages
//#import schedule
//#import sense

//import basic,generic,complex,action_pattern,blackboard,competence,competence_element,drive_collection,drive_element,messages,schedule,sense 
//from generic import *
//class Sense(Generic):
    
//    def __init__(self,
//                 sensor = None,
//                 sense_value = None,
//                 sense_predicate = None,
//                 **kw):
//        # Call ancestor init with the remaining keywords
//        Generic.__init__(self, **kw)
//        self.sensor = sensor
//        self.sense_value = sense_value
//        self.sense_predicate = sense_predicate
//        # The sensor is called when sensep need to construct a comparison
//        # using eval(self.sensor() comp_operator value) where
//        # comp_operator is the predicate(eq, lt, gt, ne).
        
//    def fire(self, timestamp, timeout_interval = DEFAULT_VIEWTIME):
//        result = self.sensep()
//        if result:
//            self.fire_postactions(timestamp = timestamp,
//                                  timeout_interval = timeout_interval,
//                                  result = "done")
//        else:
//            self.fire_postactions(timestamp = timestamp,
//                                   timeout_interval = timeout_interval,
//                                   result = "fail")
//        return result
    
//    def sensep(self):
//        if not self.sense_value:
//            return self.sensor()
//        else:
//            # print "eval('self.sensor() " + self.sense_predicate + " " + \
//            #            self.sense_value + "')"
//            # This is touchy. Due to psyco, we need to first run the
//            # sensor, and then evaluate it.
//            # return eval("self.sensor() " + self.sense_predicate + " " + \
//            #             self.sense_value)
//            result = self.sensor()
//            return eval(repr(result) + " " + self.sense_predicate + " " + \
//                        self.sense_value)
