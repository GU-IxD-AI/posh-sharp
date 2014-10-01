using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys.events

{
    /// <summary>
    /// The FireHandler is used to track all fire events created by PlanElements in a POSH plan.
    /// </summary>
    /// <param name="p">the object which triggered the event</param>
    /// <param name="f">the additional arguments which give further information about the event</param>
    public delegate void FireHandler(EventType t, object p, EventArgs f);

    public enum EventType { None, Fire, Change }
        
    public interface IListener
    {
        void Subscribe(object p);

        bool ListensFor(EventType evType);

    }
}
