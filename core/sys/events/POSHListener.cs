using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.strict;

namespace POSH.sys.events
{
    public class POSHListener : IListener
    {
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
            throw new NotImplementedException();
        }

        private void Listen(EventType t, object p, EventArgs f)
        {

        }
    }
}
