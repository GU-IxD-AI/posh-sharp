using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.operators;
using GrammarGP.elements;

namespace GrammarGP.env
{
    public class Configuration : ICloneable
    {
        public int maxCrossOverDepth { get; private set; }

        /// <summary>
        /// The mutation rate is defining the chance of a gene to mutate. The rate is given in values between [0f,1.0f].
        /// The mutation rate should be generally low as suggested by Schwefel&Baeck (1994) roughly around 0.01 or even lower.
        /// Mutation is a driving evolution but is potentially harmful to the fitness of an individuum.
        /// Mutation happens random and pointwise.
        /// </summary>
        public float mutationRate {get; private set;}

        /// <summary>
        /// The crossover rate defines the percentage of the new population which is created by crossing over two parents. 
        /// The value is between [0f,1.0f]. The remaining rate (1-xover) is filled with direct reproduction from the previous generation.
        /// The average value of crossover should be larger than 0.5f ans is default 0.6f.
        /// In GP crossover is a potential harmful operation in terms of fitness as the offspring cannot be guaranteed to be fitter than parents.
        /// </summary>
        public float crossOverRate {get; private set;}

        /// <summary>
        /// The newChromosome rate defines the absolute percentage of newly created chromosomes to be introduced into the new pool. This is 
        /// independent of all other rates and uses values between [0f,1.0f] if 1.0f is used the whole pool is completely generated anew each cycle 
        /// resulting in a pure random pool without evolution. The default value is 0.2f.
        /// </summary>
        public float newChromosomeRate { get; private set; }

        /// <summary>
        /// The probability of picking a function instead of a terminal during Crossover. The probability of selecting a Terminal is f_T = 1.0f - f_F
        /// </summary>
        public float functionProbability { get; private set; }

        public int populationSize{get; private set;}

        public ICrossOverOperator crossOverOperator { get; private set; }
        public ISelectOperator selectOperator { get; private set; }

        public Random randomGenerator { get; private set; }

        public IGenePool genePool { get; private set; }

        public int maxNodes { get; private set; }

        public int generation { get; private set; }

        public bool verbose { get; private set; }

        public IFitnessEvaluator fitnessEvaluator { get; private set; }
        
        private Configuration(Configuration oldConfig)
            : this()
        {
            this.mutationRate = oldConfig.mutationRate;
            this.crossOverRate = oldConfig.crossOverRate;
            this.newChromosomeRate = oldConfig.newChromosomeRate;
            this.populationSize = oldConfig.populationSize;
            this.genePool = oldConfig.genePool; // COMMENT: the current gene pool is just copied instead of cloned the idea behind it is that the genepool contains templates but does not change over evolution
        }

        public Configuration()
        {
            mutationRate = 0.01f;
            crossOverRate = 0.6f;
            newChromosomeRate = 0.2f;
            functionProbability = 0.75f; // This value is randomly picked without any groudning except that crossing over larger portions of the tree has a larger impact on the fitness.
            populationSize = 100;
            genePool = new GenePool();
            generation = 0;
            verbose = false;
            maxNodes = 100;
            maxCrossOverDepth = maxNodes;
            fitnessEvaluator = new MaxFitnessEvaluator();
        }

        public void InitConfiguration(Random randomGenerator, int populationSize, AGene[] geneTemplates)
        {
            this.randomGenerator = randomGenerator;
        }

        public void SetMutationRate(float rate)
        {
            mutationRate = rate;
        }

        public void SetCrossOverRate(float rate)
        {
            crossOverRate = rate;
        }

        public void SetNewChromosomeRate(float rate)
        {
            newChromosomeRate = rate;
        }

        public void SetPopulationSize(int size)
        {
            populationSize = size;
        }

        public void SetCrossOverOperator(ICrossOverOperator op)
        {
            crossOverOperator = op;
        }

        public object Clone()
        {
            return new Configuration(this);
        }

        internal void SetMaximumNodes(int maxNodes)
        {
            this.maxNodes = maxNodes;
        }

        internal void SetFitnessEvaluator(IFitnessEvaluator eval)
        {
            fitnessEvaluator = eval;
        }

        internal void SetVerbose(bool verbose)
        {
            this.verbose = verbose;
        }

        internal int GetGenerationNr()
        {
            return generation;
        }

        internal void IncreaseGeneration()
        {
            generation++;
        }

        internal void SetGeneration(int gen)
        {
            generation = gen;
        }

        internal void SetFunctionProb(float prob)
        {
            functionProbability = prob;
        }

        internal void SetMaximalCrossOverDepth(int depth)
        {
            maxCrossOverDepth = depth;
        }
    }
}
