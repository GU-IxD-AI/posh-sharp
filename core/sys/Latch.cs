using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys
{
    /// <summary>
    /// A class that implements a universal latch
    /// A latch has numerous properties:
    /// <![CDATA[lower=lower threshold. If current_state < lower, trigger=True
    /// upper=upper threshold (optional)
    /// inter=intermediate threshold (optional) for interrupts
    /// increment=positive change of current_state
    /// decrement=negative change of current_state
    /// ]]>
    /// </summary>
    public class Latch
    {
        int lower; 
        int currentState;
        int upper;
        int inter;
        int increment;
        int decrement;

        bool currentlyExectued;
        bool mayInterrupt;

        public const int NOTSET = -99999;

		public Latch(int currentState,int lower, int increment, int decrement) : this (currentState, lower, increment, decrement, NOTSET, NOTSET, false)
		{}
        public Latch(int currentState,int lower, int increment, int decrement,int upper, int inter, bool mayInterrupt)
        {
            this.lower = lower;
            this.inter = inter;
            this.upper = (upper == -99999) ? lower : upper;

            this.currentState = currentState;
            this.increment = increment;
            this.decrement = decrement;

            this.currentlyExectued = false;
            this.mayInterrupt = mayInterrupt;
        }

        public bool wantsToInterrupt()
        {
            return (currentState < 20) ? true : false;
        }

        public void decrementCurrentState()
        {
            currentState -= decrement;
        }

        public void incrementCurrentState()
        {
            currentState += increment;
        }

        public int getCurrentState()
        {
            return currentState;
        }

        public void setCurrentState(int newCurrentState)
        {
            currentState = newCurrentState;
        }

        public bool isSaturated()
        {
            if (currentState >= upper)
            {
                currentlyExectued = false;
                return true;
            }

            return false;
        }

        public bool isTriggered()
        {
            return (currentlyExectued || currentState < lower);
        }

        public bool signalInterrupt()
        {
            if (inter != NOTSET && currentState > inter)
            {
                currentlyExectued = false;
                return true;
            }

            return false;
        }

        public bool failed()
        {
            return (currentState <= 0 );
        }

        public void activate()
        {
            currentlyExectued = true;
        }

        public void deactivate()
        {
            currentlyExectued = false;
        }

        public bool active()
        {
            return currentlyExectued;
        }

        public void resetAgent()
        {
            currentlyExectued = false;
            if (upper != NOTSET)
                currentState = upper;
            else
                currentState = 2 * lower;
        }

        public float getUrgency()
        {
            return (1.0f - ((float)currentState / (float)lower));
        }
    }
}

