using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH.sys.events
{
    class SenseArgs : FireArgs
    {
        private object senseResult;
        public object Sensed
        {
            set
            {
                senseResult = value;
            }
            get
            {
                return this.senseResult;
            }
        }
    }
}
