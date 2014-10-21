using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;
using GrammarGP.elements;

namespace GrammarGP.operators
{
    public class WeightedRoulette : ISelectOperator
    {
        private Configuration   m_config;
        private IProgramPool    m_progPool;
        private Dictionary<IProgram,SlotCounter> m_wheel;
        private List<SlotCounter>   m_slotPool;

        private int m_generationCounter;

        // the following 4 parameters are only arround to control a stagnating pool if a local optima is reached and not left
        // in this case we shift the mutation rate and chromosome percentage to kickstart the pool by introducing more randomness (which is not always good)
        private double  inital_MutationRate;
        private double  inital_NewChromosomePercentage;
        private double  prev_largest = 0;
	    private int     prev_largest_gen = 0;

        //delta for distinguishing whether a value is to be interpreted as zero
	    private const double DELTA = 0.000001d;

        private int     m_activeGenotypeID;

        private double  m_totalNumberOfUsedSlots;

        public WeightedRoulette(Configuration config)
        {
            m_config    = config;
            m_progPool  = new ProgramPool();
            m_wheel     = new Dictionary<IProgram,SlotCounter>();
		    m_slotPool  = new List<SlotCounter>();
		    
		    m_generationCounter = 0;
		    inital_MutationRate = m_config.mutationRate;
            inital_NewChromosomePercentage = m_config.newChromosomeRate;
        }

        public void AddPrograms(IProgram[] progs)
        {
            for (int i = 0; i < progs.Length; i++)
                AddProgram(progs[i]);
        }

        public void AddProgram(IProgram prog)
        {
            // The "roulette wheel" is represented by a Map. Each key is a
            // Chromosome and each value is an instance of the SlotCounter inner
            // class. The counter keeps track of the total number of slots that
            // each chromosome is occupying on the wheel (which is equal to the
            // combined total of their fitness values). If the Chromosome is
            // already in the Map, then we just increment its number of slots
            // by its fitness value. Otherwise we add it to the Map.
            // -----------------------------------------------------------------
            SlotCounter counter = (SlotCounter)m_wheel[prog];
            if (counter != null)
            {
                // The Chromosome is already in the map.
                // -------------------------------------
                counter.Increment();
            }
            else
            {
                // We're going to need a SlotCounter. See if we can get one
                // from the pool. If not, construct a new one.
                // --------------------------------------------------------
                counter = (SlotCounter)m_slotPool.Last();
                if (counter == null)
                {
                    counter = new SlotCounter();
                }
                else
                {
                    m_slotPool.Remove(counter);
                }
                counter.Reset(prog.GetFitnessValue());
                m_wheel[prog] = counter;
            }
        }

        public void SetConfiguration(env.Configuration config)
        {
            throw new NotImplementedException();
        }

        public IProgram SelectIndividuum(GenoType genotype)
        {
        
		// this should only be called once per generation to set up a new roulette wheel
		if (this.m_generationCounter <= genotype.GetConfiguration().GetGenerationNr() && this.m_activeGenotypeID != genotype.GetHashCode()){
			
            m_activeGenotypeID = genotype.GetHashCode();
			Empty();
			AddPrograms(genotype.GetAllPrograms());
            //Log(String.format("num of chromosomes: %d",pop.getGPPrograms().length));
			ScaleFitness();
			m_generationCounter++;
		}
		
		// Build three arrays from the key/value pairs in the wheel map: one
		// that contains the fitness values for each igpprogram, one that
		// contains the total number of occupied slots on the wheel for each
		// igpprogram, and one that contains the igpprogram themselves. The
		// array indices are used to associate the values of the three arrays
		// together (eg, if a igpprogram is at index 5, then its fitness value
		// and counter values are also at index 5 of their respective arrays).
		// -------------------------------------------------------------------

		List<double> fitnessValues = new List<double>();
		List<double> counterValues = new List<double>();
		List<IProgram> programs = new List<IProgram>();
		m_totalNumberOfUsedSlots = 0.0d;

		foreach (KeyValuePair<IProgram, SlotCounter> prog in m_wheel) {
			fitnessValues.Add(prog.Value.GetFitnessValue());
			counterValues.Add(prog.Value.GetFitnessValue() * prog.Value.GetCounterValue());
			programs.Add(prog.Key);
			// We're also keeping track of the total number of slots,
			// which is the sum of all the counter values.
			// ------------------------------------------------------
			m_totalNumberOfUsedSlots +=counterValues.Last();
		}
		// To select each igpprogram, we just "spin" the wheel and grab
		// whichever igpprogram it lands on.
		// ------------------------------------------------------------
		IProgram selectedIGPprog = SpinWheel(m_config.randomGenerator, fitnessValues, counterValues,
				programs);

		return selectedIGPprog;
        }

        public bool Reset()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Empty the working pool of Chromosomes by putting all counters into the slotPool and clearing the wheel
        /// </summary>
        public void Empty()
        {
            m_slotPool.AddRange(m_wheel.Values);
            m_wheel.Clear();
            m_totalNumberOfUsedSlots = 0;

        }

        private  void ScaleFitness()
        {
            // First, add up all the fitness values. While we're doing this,
		    // keep track of the largest fitness value we encounter.
		    // -------------------------------------------------------------
		    double largestFitnessValue = 0.0;
		    decimal totalFitness = 0;
		    
            foreach (SlotCounter counter in m_wheel.Values) {
			    if (counter.GetFitnessValue() > largestFitnessValue) {
				    largestFitnessValue = counter.GetFitnessValue();
			    }
                try {
                    decimal counterFitness = (decimal)counter.GetFitnessValue();
                    totalFitness = totalFitness + ( counterFitness * counter.GetCounterValue() );
                }
			    catch (NotFiniteNumberException n){
                    Console.Error.WriteLine(n);
                    Console.Error.WriteLine("---err-wrong-fitness--");
                    Console.Error.WriteLine("--- "+ counter.GetFitnessValue() +" --");
                    Console.Error.WriteLine("---err-wrong-fitness--");
                    // TODO: include logger again
                    // Logger.getLogger("genotype.log").log(Level.ALL, "-err-wrong-fitness-" + counter.getCounterValue());
                    totalFitness = totalFitness + ((decimal)0.00001f * counter.GetCounterValue());
                }
		    }
		    /* storing the previous best fitness and its generation */
		    if (largestFitnessValue > prev_largest){
			    prev_largest = largestFitnessValue;
			    prev_largest_gen = m_generationCounter;
		    } 
		
		    int unChangedFitness = m_generationCounter - prev_largest_gen;
		    if (m_generationCounter >= 50 && unChangedFitness <= 100) {
			    m_config.SetMutationRate((float) inital_MutationRate);
			    m_config.SetNewChromosomeRate((float) inital_NewChromosomePercentage);
			}
		    /* if the crossover did not bring any useful results for the previous 100 generations try to 
		     * increase the mutation probability to bring a greater varierty of chromosomes into the pool
		     * there is a danger of mutating too much if no good solution is found within the next 50 generations*/
		
		    if (unChangedFitness > 100 && m_config.mutationRate < this.inital_MutationRate) {
			    double prob = m_config.mutationRate;
			    Console.Out.WriteLine(String.Format("==mutation: %05f=============",prob));
			    m_config.SetMutationRate((float)(prob+prob*.01f));
			    prob = m_config.newChromosomeRate;
			    Console.Out.WriteLine(String.Format("==new chrom: %05f=============",prob));
			    m_config.SetNewChromosomeRate((float)(prob+prob*.01f));
		    }

		    // Now divide the total fitness by the largest fitness value to
		    // compute the scaling factor.
		    // ------------------------------------------------------------
		    if (largestFitnessValue > 0.000000d
				    && (float)totalFitness > 0.0000001d) {
			    double scalingFactor = (double) (totalFitness / (decimal)largestFitnessValue );
			    // Divide each of the fitness values by the scaling factor to
			    // scale them down.
			    // ----------------------------------------------------------
			    Console.Out.WriteLine(String.Format("==gen: %05d=============",m_generationCounter));
			    Console.Out.WriteLine("scaling factor: "+scalingFactor);
			    Console.Out.WriteLine("total fitness: "+totalFitness);
			    Console.Out.WriteLine(String.Format("largest fitness: %.6f  since gen: %d", largestFitnessValue, prev_largest_gen));
			    Console.Out.WriteLine("avg fitness: "+totalFitness/m_wheel.Count);
			    Console.Out.WriteLine("=========================");
			
			    foreach (SlotCounter counter in m_wheel.Values) 
				    counter.ScaleFitnessValue(scalingFactor);

		    }
        }

        private IProgram SpinWheel(Random randomGenerator,
            List<double> a_fitnessValues,
            List<double> a_counterValues,
            List<IProgram> a_programs)
        {

            double selectedSlot = randomGenerator.NextDouble() * m_totalNumberOfUsedSlots;
            if (selectedSlot > m_totalNumberOfUsedSlots)
            {
                selectedSlot = m_totalNumberOfUsedSlots;
            }
            // Loop through the wheel until we find our selected slot. Here's
            // how this works: we have three arrays, one with the fitness values
            // of the chromosomes, one with the total number of slots on the
            // wheel that each chromosome occupies (its counter value), and
            // one with the chromosomes themselves. The array indices associate
            // each of the three together (eg, if a chromosome is at index 5,
            // then its fitness value and counter value are also at index 5 of
            // their respective arrays).
            //
            // We've already chosen a random slot number on the wheel from which
            // we want to select the Chromosome. We loop through each of the
            // array indices and, for each one, we add the number of occupied slots
            // (the counter value) to an ongoing total until that total
            // reaches or exceeds the chosen slot number. When that happens,
            // we've found the chromosome sitting in that slot and we return it.
            // --------------------------------------------------------------------
            double currentSlot = 0.0d;
            IFitnessEvaluator evaluator = m_config.fitnessEvaluator;
            bool isFitter2_1 = evaluator.IsFitter(2, 1);
            for (int i = 0; i < a_counterValues.Count; i++)
            {
                // Increment our ongoing total and see if we've landed on the
                // selected slot.
                // ----------------------------------------------------------
                bool found;
                if (isFitter2_1)
                {
                    // Introduced DELTA to fix bug 1449651
                    found = selectedSlot - currentSlot <= DELTA;
                }
                else
                {
                    // Introduced DELTA to fix bug 1449651
                    found = Math.Abs(currentSlot - selectedSlot) <= DELTA;
                }
                if (found)
                {
                    // Remove one instance of the chromosome from the wheel by
                    // decrementing the slot counter by the fitness value resp.
                    // resetting the counter if doublette chromosomes are not
                    // allowed.
                    // -------------------------------------------------------
                    //TODO: check why the wheel is modified during selection what is the benefit of the modification
                    //if (!getDoubletteChromosomesAllowed()) {
                    //    m_totalNumberOfUsedSlots -= a_counterValues.get(i);
                    //    a_counterValues.set(i, 0d);
                    //}
                    //else {
                    // commented because if doublettes are allowed reducing the fitness alters the selection process
                    //a_counterValues.set(i, a_counterValues.get(i)- a_fitnessValues.get(i));
                    //m_totalNumberOfUsedSlots -= a_fitnessValues.get(i);
                    //}
                    // Introduced DELTA to fix bug 1449651
                    if (Math.Abs(m_totalNumberOfUsedSlots) < DELTA)
                    {
                        m_totalNumberOfUsedSlots = 0.0d;
                    }
                    // Now return our selected Chromosome.
                    // -----------------------------------
                    return a_programs[i];
                }
                else
                {
                    currentSlot += a_counterValues[i];
                }
            }
            // We have reached here because there were rounding errors when
            // computing with doubles or because the last entry is the right one.
            // ------------------------------------------------------------------
            return a_programs[a_counterValues.Count - 1];
        }
    }
}
