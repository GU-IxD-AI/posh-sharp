using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.sys
{
    class LatchedBehaviour : Behaviour
    {
        private Latch latch;

        public LatchedBehaviour(AgentBase agent, string[] actions, string[] senses, 
            Dictionary<string, object> attributes = null, Behaviour caller = null)
            : base(agent,actions,senses,attributes,caller)
        {
            
        }

        public void setLatch(int currentState, int lower, int increment, int decrement, int upper = Latch.NOTSET, 
            int inter = Latch.NOTSET, bool mayInterrupt = false)
        {
            latch = new Latch(currentState, lower, increment, decrement, upper, inter, mayInterrupt);
        }

        public bool wantsToInterrupt()
        {
            return latch.wantsToInterrupt();
        }

        public void decrementCurrentState()
        {
            latch.decrementCurrentState();
        }

        public void incrementCurrentState()
        {
            latch.incrementCurrentState();
        }

        public int getCurrentState()
        {
            return latch.getCurrentState();
        }

        public void setCurrentState(int newCurrentState)
        {
            latch.setCurrentState(newCurrentState);
        }

        public bool isSaturated()
        {
            return latch.isSaturated();
        }

        public bool isTriggered()
        {
            return latch.isTriggered();
        }

        public bool signalInterrupt()
        {
            return latch.signalInterrupt();
        }

        public bool failed()
        {
            return latch.failed();
        }

        public void activate()
        {
            latch.activate();
        }

        public void deactivate()
        {
            latch.deactivate();
        }

        public bool active()
        {
            return latch.active();
        }

        public void resetAgent()
        {
            latch.resetAgent();
        }

        public float getUrgency()
        {
            return latch.getUrgency();
        }
    }
}
