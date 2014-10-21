using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarGP.elements
{
    class ProgramPool : IProgramPool
    {
        private Dictionary<string, IProgram> m_pool;

        public ProgramPool()
        {
            m_pool = new Dictionary<string, IProgram>();
        }

        public ProgramPool(IProgram [] progs)
        {
            m_pool = new Dictionary<string, IProgram>();
            for (int i = 0; i < progs.Length; i++)
                m_pool.Add(progs[i].GetAgent().id,progs[i]);
        }

        public bool AddProgram(IProgram prog)
        {
            if (!m_pool.ContainsKey(prog.GetAgent().id))
            {
                m_pool[prog.GetAgent().id] = prog;
                return true;
            }

            return false;
        }

        public IProgram GetProgram(string agentID)
        {
            return (m_pool.ContainsKey(agentID)) ? m_pool[agentID] : null;
        }

        public bool RemoveProgram(string agentID)
        {
            return (m_pool.ContainsKey(agentID)) ? m_pool.Remove(agentID) : false;
        }

        public bool RemoveProgram(IProgram prog)
        {
            return (m_pool.ContainsKey(prog.GetAgent().id)) ? m_pool.Remove(prog.GetAgent().id) : false;
        }
    }
}
