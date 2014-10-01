using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys
{
    public class RampActivation
    {
        private int stickiness; 
        private int saturation;
        private int lower;
        private float increment;
        private float urgency;
        private float activation;
        private bool active;
        private int interrupts;
        private int count;

        /// <summary>
        /// Class impementing ERGo, a ramp based activation function to monitor the execution of parallel behaviours
        /// </summary>
        /// <param name="activation"></param>
        /// <param name="stickiness"></param>
        /// <param name="lower"></param>
        /// <param name="increment"></param>
        /// <param name="urgency_multiplier"></param>
        public RampActivation(int activation, int stickiness, int lower, float increment, float urgency_multiplier, int interrupts)
        {
            this.activation = activation;
            this.stickiness = stickiness;
            this.saturation = stickiness;
            this.lower = lower;
            this.increment = increment;
            this.urgency = urgency_multiplier;
            this.active = false;
            this.interrupts = interrupts;
            this.count = 0;
        }

        public bool IsActive()
        {
            return active;
        }

        public float GetActivation()
        {
            return this.activation;
        }

        public void Tick(bool urgent){
            if (active)
                activation += increment*urgency;
            else
                activation += increment;

            if (urgent)
                activation = activation * urgency;
        }

        public void Reset()
        {
            saturation = stickiness;
            activation = lower;
            active = false;
        }

        public void ReachedGoal()
        {
            if (!active)
                return;

            if (saturation > 0)
                saturation -= 1;
            else
                activation = lower;
            
        }

        public bool Switch()
        {
            if (active)
            {
                if (interrupts < 0 || count < interrupts ||saturation <= 0)
                {
                    active = false;
                    saturation = stickiness;
                    count = 0;
                    return true;
                }

                count++;
            }

            return false;
        }

        public bool Challenge(Dictionary<string,RampActivation> behaviourActivations)
        {
            bool noAct = true;
            RampActivation strongest = this;

            foreach (RampActivation current in behaviourActivations.Values)
            {
                if (current == strongest)
                    continue;
                if (strongest.activation < current.activation)
                    strongest = current;
                else if (current.active)
                    noAct = current.Switch();
            }
            if (this == strongest && noAct)
                this.active = true;

            return active;    
        }


    }
}

		

		



