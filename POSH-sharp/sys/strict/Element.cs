using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace POSH_sharp.sys.strict
{
    /// <summary>
    /// A basic POSH element.
    ///
    /// A basic POSH element is any plan / behaviour element, like a drive,
    /// a drive element, an action pattern, a sense, ...
    ///
    /// Each such an element has a unique numeric id, that is
    /// assigned to the element upon creating it.
    ///
    /// This element is not used directly, but is inherited
    /// by L{POSH.strict.Sense}, L{POSH.strict.Action}, and
    /// L{POSH.strict.PlanElement}.
    /// </summary>
    class ElementBase : LogBase
    {
        static int currentId = 0;

        /// <summary>
        /// Returns a unique element id.
        /// This function returns an id for plan elements. At every call, 
        /// the internal id counter is increased by 1.
        /// </summary>
        /// <returns>A unique element id.</returns>
        static int getNextId(){
            return currentId += 1;
        }

        

        public void init(Agent agent, Log logDomain){

        }

    }

class ElementBase(LogBase):

    def __init__(self, agent, log_domain):
        """Initialises the element, and assigns it a unique id.
        
        @param agent: The agent that uses the element.
        @type agent: L{POSH.strict.Agent}
        @param log_domain: The logging domain for the element.
        @type log_domain: string
        """
        LogBase.__init__(self, agent, log_domain)
        self._id = _get_next_id()
        self._name = "NoName"
                
    def getName(self):
        """Returns the name of the element.
        
        The name has to be set by overriding classes by setting
        the object variable C{self._name}
        
        @return: The element's name.
        @rtype: string
        """
        return self._name
    
    def __str__(self):
        """Returns the string representation of the element.
        
        @return: '[Classname] [Elementname]'
        @rtype: string
        """
        return "%s %s" (self.__class__.__name__, self._name)

    def getId(self):
        """Returns the element's id.

        @return: The element's id.
        @rtype: int
        """
        return self._id


class FireResult:
    """The result of firing a plan element.
    
    This result determines two things:
        
        - if we want to continue executing this part of the plan or want to
          return to the root.
        
        - the plan element to execute in the next step, given that we are
          continuing to execute the current plan of the step.
    
    Continuing the execution means to either to fire the
    same element in the next execution step, or to descend further
    in the plan tree and fire the next element in that tree.
    The next element to execute also needs to be given. If this element
    is set to None, the element to execute stays the same. Otherwise
    the given element is copied, reset, and given as the next element.
    
    If we are not continuing the execution of the current part of the
    plan, the currently fired drive element returns to the root of the plan.
    """
    def __init__(self, continue_execution, next_element):
        """Initialises the result of firing an element.
        
        For a more detailed description of the arguments, read the
        class documentation.
        
        @param continue_execution: If we want to continue executing the current
            part of the plan.
        @type continue_execution: boolean
        @param next_element: The next plan element to fire.
        @type next_element: None or L{POSH.strict.ElementCollection}
        """
        self._continue = continue_execution
        if continue_execution and next_element:
            # copy the next element, if there is one
            self._next = next_element.copy()
        else:
            self._next = None
        self._next = next_element
        
    def continueExecution(self):
        """Returns if we want to continue execution the current part of the
        plan.
        
        @return: If we want to continue execution.
        @rtype: boolean
        """
        return self._continue
    
    def nextElement(self):
        """Returns the element to fire at the next step.
        
        @return: Element to fire at the next step.
        @rtype: None or L{POSH.strict.ElementCollection}
        """
        return self._next




class CopiableElement(ElementBase):
    """An element that can be copied.

    Any element that is or can become the element of a drive element
    or competence element as to be of this class or a inheriting class.
    """
    def __init__(self, agent, log_domain):
        """Initialises the element.
        
        @param agent: The agent that uses the element.
        @type agent: L{POSH.strict.Agent}
        @param log_domain: The logging domain for the element.
        @type log_domain: string
        """
        ElementBase.__init__(self, agent, log_domain)

    def copy(self):
        """Returns a reset copy of itself.
        
        This method returns a copy of itself, by creating a new
        instance of itsself and replicating all state-dependent object
        variables. If the object variables are not state-dependent,
        they can be copied as references rather than real copies.
        
        This method needs to be overriddent by inheriting classes.
        In its current implementation it raises NotImplementedError
        
        @return: A copy of itsself.
        @rtype: self.__class__
        @raise NotImplementedError: always
        """
        raise NotImplementedError, \
            "CopiableElement.copy() needs to be overridden"


class PlanElement(CopiableElement):
    """An element of a POSH plan.
    """
    def __init__(self, agent, log_domain):
        """Initialises the element.
        
        @param agent: The agent that uses the element.
        @type agent: L{POSH.strict.Agent}
        @param log_domain: The logging domain for the element.
        @type log_domain: string
        """
        CopiableElement.__init__(self, agent, log_domain)
        
    def reset(self):
        """Resets the element.
        
        This method has to be overridden in inheriting classes.
        In its default implementation is raises NotImplementedError.
        
        @raise NotImplementedError: always
        """
        raise NotImplementedError, \
            "PlanElement.reset() needs to be overridden"
    
    def fire(self):
        """Fires the element and returns the result.
        
        The result is given as a FireResult object.
        This method needs to be overriden by inheriting classes.
        In its default implementation is raises NotImplementedError.
        
        @return: The result of firing the element.
        @rtype: L{POSH.strict.FireResult}
        @raise NotImplementedError: always
        """
        raise NotImplementedError, \
            "PlanElement.fire() needs to be overridden"

    
class Element(PlanElement):
    """A simple POSH plan element.
    
    This element has besides the L{PlanElement} an additional
    ready-state that is queried before it is fired.
    """
    def __init__(self, agent, log_domain):
        """Initialises the element.
        
        @param agent: The agent that uses the element.
        @type agent: L{POSH.strict.Agent}
        @param log_domain: The logging domain for the element.
        @type log_domain: string
        """
        PlanElement.__init__(self, agent, log_domain)
    
    def isReady(self, timestamp):
        """Returns if the element is ready to be fired.
        
        This method needs to be overridden by inheriting classes.
        In its default implementation it raises NotImplementedError.
        
        @param timestamp: The current timestamp in milliseconds.
        @type timestamp: long
        @return: If the element can be fired.
        @rtype: boolean
        @raise NotImplementedError: always
        """
        raise NotImplementedError, \
            "Element.isReady() needs to be overridden"
    

class ElementCollection(PlanElement):
    """A collection of POSH plan elements.
    
    This collection provides the same functionality as L{POSH.strict.PlanElement}.
    """
    def __init__(self, agent, log_domain):
        """Initialises the element collection.
        
        @param agent: The agent that uses the element collection.
        @type agent: L{POSH.strict.Agent}
        @param log_domain: The logging domain for the element collection.
        @type log_domain: string
        """
        PlanElement.__init__(self, agent, log_domain)

}
