using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys.events
{
    class FireArgs : EventArgs
    {
        private DateTime TimeNow;
        public DateTime Time
        {
            set
            {
                TimeNow = value;
            }
            get
            {
                return this.TimeNow;
            }
        }
        private bool fireResult;
        public bool FireResult
        {
            set
            {
                fireResult = value;
            }
            get
            {
                return this.fireResult;
            }
        }
    }
}
