using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.events;

namespace POSH.sys.strict
{
    /// <summary>
    /// A conjunction of senses and sense-acts, acting as a trigger.
    /// </summary>
    public class Trigger : ElementBase
    {
        protected internal POSHSense [] senses;

        public static string [] getNames(ElementBase [] elements)
        {
            string [] result = new string[elements.Length];
            for (int i = 0; i < elements.Length; i++)
                result[i] = elements[i].getName();

            return result;
        }


        /// <summary>
        /// Initialises the trigger.
        /// 
        /// The log domain is set to [Agent].T.[senseName1+senseName2+...]
        /// </summary>
        /// <param name="agent">The agent that uses the trigger.</param>
        /// <param name="senses">The list of senses and sense-acts for the trigger.</param>
        public Trigger(Agent agent,POSHSense []senses)
            : base(string.Format("T.{0}",string.Join("+",getNames(senses))),agent)
        {
            this.senses = senses;
            log.Debug("Created");


        }

        /// <summary>
        /// Fires the trigger.
        /// 
        /// The trigger returns True of all senses/sense-acts of the
        /// trigger evaluate to True.
        /// </summary>
        /// <returns>If all the senses/sense-acts evaluate to True.</returns>
        public bool fire()
        {
            bool success = true; 
            
            log.Debug("Firing");
            foreach (POSHSense sense in this.senses)
                if (!sense.fire().continueExecution())
                {
                    log.Debug(string.Format("Sense {0} failed",sense.getName()));
                    success = false;
                    break;
                }
            
            // logging the event
            FireArgs args = new FireArgs();
            args.FireResult = success;
            args.Time = DateTime.Now;

            BroadCastFireEvent(args);

            return success;
        }

        public override string ToSerialize(Dictionary<string,string> elements)
        {
            string plan = "";
            elements = (elements is Dictionary<string,string>)? elements : new Dictionary<string,string>();

            // taking appart the senses and putting them into the right form
            plan = "( ";
            for (int i = 0; i < senses.Length; i++)
            {
                plan += senses[i].ToSerialize(elements)+" ";
            }
            plan += ")";

            return plan;
        }
    }
}