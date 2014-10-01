using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH.sys.strict;

namespace POSH.sys.parse
{
    public class LAPWriter
    {
        string lapFile;
       
        public bool done { get; private set; }
        POSH.sys.scheduled.DriveCollection lapDC;
        POSH.sys.strict.DriveCollection lapSDC;

        public LAPWriter()
        {
            lapFile = "";
            done = false;
            lapDC = null;
            lapSDC = null;
        }

        public void PrepareStrictLAP(AgentBase agent)
        {
            done = false;
            if (agent is Agent)
            {
                lapSDC = (agent as Agent).dc;
                DismantleSDC();
            }

        }

        public void PrepareScheduledLap(AgentBase agent)
        {
            done = false;
            if (agent is Agent)
            {
                lapDC = (agent as POSH.sys.scheduled.Agent).dc;
                //DismantleDC();
            }
            //TODO: needs to be done once scheduled mode works in POSH#
        }

        protected void DismantleSDC()
        {
            lapFile = "(\n" + lapSDC.ToSerialize(new Dictionary<string,string>()) + "\n)";

            done = true;
        }

        public void Reset()
        {
            done = false;
            lapFile = null;
            lapDC = null;
            lapSDC = null;
        }

        public string GetLapString()
        {
            return (done) ? lapFile : string.Empty;
        }

    }
}
