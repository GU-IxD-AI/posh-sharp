using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.strict;

namespace POSH.sys.events
{
    public class POSHListener : IListener
    {

        public Stack<Tuple<EventType, object, EventArgs>> eventStack;

        public POSHListener()
        {
            eventStack = new Stack<Tuple<EventType, object, EventArgs>>();
        }

        public void Subscribe(object p)
        {
            if (p is PlanElement)
            {
                PlanElement pE = p as PlanElement;
                pE.FireEvent += new FireHandler(Listen);
            }
        }

        public bool ListensFor(EventType evType)
        {
            return (evType == EventType.Fire) ? true : false;
        }

        private void Listen(EventType t, object p, EventArgs f)
        {
            eventStack.Push(new Tuple<EventType, object, EventArgs>(t, p, f));
        }
    }
}
