using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.elements;

namespace GrammarGP.env
{
    public class Population : IPopulation,ICloneable
    {
        public Configuration gpConfiguration { get; private set; }
        public IChromosomePool chromPool { get; private set; }
        public IProgramPool programmPool { get; private set; }

		public const Double DELTA = 0.0000001d;

		[NonSerialized]
		private Logger LOGGER = Logger.getLogger(this.GetType());

		[NonSerialized]
		private int warningPrototypeReused = 0;

		/**
   * The array of GPProgram's that make-up the Genotype's population.
   */
		private IGPProgram[] m_programs;

		private float[] m_fitnessRank;

		private int m_popSize;

		[NonSerialized]
		private transient IGPProgram m_fittestProgram;

		private GPConfiguration m_config;

		/**
   * Indicates whether at least one of the programs has been changed
   * (deleted, added, modified).
   */
		private boolean m_changed;

		/**
   * Indicates that the list of GPPrograms has been sorted.
   */
		private boolean m_sorted;

		private IGPProgram m_fittestToAdd;



        private Population(Population oldPop)
            : this((Configuration) oldPop.gpConfiguration.Clone())
        {

        }

        public Population(Configuration conf)
        {
            this.gpConfiguration = conf;
        }

    
        public object  Clone()
        {
            return new Population(this);
        }

		public void Clear() {
			for (int i = 0; i < m_programs.length; i++) {
				m_programs[i] = null;
			}
			m_changed = true;
			m_sorted = true;
			m_fittestProgram = null;
		}

		public boolean IsFirstEmpty() {
			if (size() < 1) {
				return true;
			}
			if (m_programs[0] == null) {
				return true;
			}
			return false;
		}

		public String GetPersistentRepresentation() {
			StringBuffer b = new StringBuffer();
			for (IGPProgram program : m_programs) {
				b.append(GPPROGRAM_DELIMITER_HEADING);
				b.append(encode(
					program.getClass().getName() +
					GPPROGRAM_DELIMITER +
					program.getPersistentRepresentation()));
				b.append(GPPROGRAM_DELIMITER_CLOSING);
			}
			return b.toString();
		}

    }   
}
