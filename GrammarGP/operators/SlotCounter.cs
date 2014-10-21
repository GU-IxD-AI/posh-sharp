using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarGP.operators
{

    /// <summary>
    /// Implements a counter that is used to keep track of the total number of
    /// slots that a single Chromosome is occupying in the roulette wheel. Since
    /// all equal copies of a chromosome have the same fitness value, the increment
    /// method always adds the fitness value of the chromosome. Following
    /// construction of this class, the reset() method must be invoked to provide
    /// the initial fitness value of the Chromosome for which this SlotCounter is
    /// to be associated. The reset() method may be reinvoked to begin counting
    /// slots for a new Chromosome.
    /// 
    /// @author Swenm Gaudl
    /// @author Neil Rotstan (used the whole class directly from JGAP project)
    /// </summary>
    public class SlotCounter : ICloneable
    {
        /// <summary>
        /// The fitness value of the Chromosome for which we are keeping count of
        /// roulette wheel slots. Although this value is constant for a Chromosome,
        /// it's not declared final here so that the slots can be reset and later
        /// reused for other Chromosomes, thus saving some memory and the overhead
        /// of constructing them from scratch.
	    /// </summary>
        private double m_fitnessValue;

        /// <summary>
        /// The current number of Chromosomes represented by this counter.
	    /// </summary>
	    private int m_count;

        /// <summary>
        /// Allows SlotCounter to be cloned and used in deep copies of the roulette wheel
        /// @author Swen Gaudl
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            SlotCounter clone = new SlotCounter();
		    clone.m_fitnessValue = this.m_fitnessValue;
		    clone.m_count = this.m_count;
		    return clone;
	    }

        /// <summary>
        /// Resets the internal state of this SlotCounter instance so that it can
        /// be used to count slots for a new Chromosome.
        /// </summary>
        /// <param name="a_initialFitness">a_initialFitness the fitness value of the Chromosome for which this instance is acting as a counter</param>
        public void Reset(double a_initialFitness) {
		    m_fitnessValue = a_initialFitness;
		    m_count = 1;
	    }
	


	    /// <summary>
	    /// Retrieves the fitness value of the chromosome for which this instance
        /// is acting as a counter.
        /// </summary>
	    /// <returns>the fitness value that was passed in at reset time</returns>
	    public double GetFitnessValue() {
		    return m_fitnessValue;
	    }

	    /// <summary>
	    /// Increments the value of this counter by the fitness value that was
        /// passed in at reset time.
	    /// </summary>
	    public void Increment() {
		    m_count++;
	    }

	    /// <summary>
	    /// Retrieves the current value of this counter: ie, the number of slots
        /// on the roulette wheel that are currently occupied by the Chromosome
        /// associated with this SlotCounter instance.
	    /// </summary>
	    /// <returns>the current value of this counter</returns>
	    public int GetCounterValue() {
		    return m_count;
	    }

	    /**
	     * Scales this SlotCounter's fitness value by the given scaling factor.
	     *
	     * @param a_scalingFactor the factor by which the fitness value is to be
	     * scaled
	     *
	     * @author Neil Rotstan
	     * @since 1.0
	     */
        public void ScaleFitnessValue(double a_scalingFactor)
        {
            m_fitnessValue /= a_scalingFactor;
        }

    }
}
