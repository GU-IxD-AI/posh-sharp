using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys.strict
{
    _intMatcher = re.compile(r'^(0|\-?[1-9]\d*|0[0-7]+|0[xX][0-9a-fA-F]+)[lL]?$')
_floatMatcher = re.compile(r'^\-?(\d*\.\d+|\d+\.)([eE][\+\-]?\d+)?$')
_boolMatcher = re.compile(r'^[Tt]rue|[Ff]alse$')



    /// <summary>
    /// A sense / sense-act as a thin wrapper around a behaviour's
    /// sense / sense-act method.
    /// </summary>
    class Sense
    {
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
        /// <param name="sense_name">The name of the sense</param>
        /// <param name="value">The value to compare it to. This is given as a string,
        /// but will be converted to an integer or float or boolean, or
        /// left as a string, whatever is possible. If None is given
        /// then the sense has to evaluate to True.</param>
        /// <param name="predicate">"==", "!=", "<", ">", "<=", ">=". If null is
        ///    given, then "==" is assumed.</param>
        public void init(Agent agent, string sense_name, string value = null, string predicate = null){

        }
    }

        ElementBase.__init__(self, agent, "Sense.%s" % sense_name)
        beh_dict = agent.getBehaviourDict()
        self._sense = beh_dict.getSense(sense_name)
        self.behaviour = beh_dict.getSenseBehaviour(sense_name)
        self._name = "%s.%s" % (self.behaviour.getName(), sense_name)
        self._value = self._convertValue(value)
        if not predicate:
            self._pred = "=="
        else:
            self._pred = predicate
        self.log.debug("Created")
    
    def fire(self):
        """Activates the sense and returns its result.
        
        @return: The result of the sense.
        @rtype: boolean
        """
        self.log.debug("Firing")
        pred, value, result = self._pred, self._value, self._sense()
        if value == None:
            return bool(result)
        elif pred == "==":
            return result == value
        elif pred == "!=":
            return result != value
        elif pred == "<=":
            return result <= value
        elif pred == ">=":
            return result >= value
        elif pred == ">":
            return result > value
        elif pred == "<":
            return result < value
        else:
            return bool(result)
    
    def _convertValue(self, value):
        """Converts the given string to whatever is possible.

        @param value: The value to convert.
        @type value: string
        @return: The same value, only converted.
        @rtype: int, float, bool, string or None
        """
        if not value:
            return None
        elif _intMatcher.match(value):
            return int(value)
        elif _floatMatcher.match(value):
            return float(value)
        elif _boolMatcher.match(value):
            return bool(value)
        else:
            return value







    
class Trigger(ElementBase):
    """A conjunction of senses and sense-acts, acting as a trigger.
    """
    def __init__(self, agent, senses):
        """Initialises the trigger.

        The log domain is set to [Agent].T.[senseName1+senseName2+...]

        @param agent: The agent that uses the trigger.
        @type agent: L{POSH.strict.Agent}
        @param senses: The list of senses and sense-acts for the trigger.
        @type senses: Sequence of L{POSH.strict.Sense}
        """
        ElementBase.__init__(self, agent, "T.%s" % \
                             '+'.join(map(Sense.getName, senses)))
        self._senses = senses
        self.log.debug("Created")

    def fire(self):
        """Fires the trigger.

        The trigger returns True of all senses/sense-acts of the
        trigger evaluate to True.

        @return: If all the senses/sense-acts evaluate to True.
        @rtype: boolean
        """
        self.log.debug("Firing")
        for sense in self._senses:
            if not sense.fire():
                self.log.debug("Sense '%s' failed" % sense.getName())
                return False
        return True


}
