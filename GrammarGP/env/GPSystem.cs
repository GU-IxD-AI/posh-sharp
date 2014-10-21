using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using GrammarGP.elements.POSH;
using POSH.sys;
using GrammarGP.elements;


namespace GrammarGP.env
{
    /// <summary>
    /// entry point for the GP system:\n
    /// </summary>
    public class GPSystem 
    {
        public Random m_randomGenerator;
        private Thread m_gpThread;
        private bool working;

        private Configuration m_config;

        /// <summary>
        /// reads a correctly formated LAP file and creates a builder
        /// correctly works when calling the Parse method with a string representation of the plan
        /// </summary>
        private GPLapReader m_reader;

        private Dictionary<string,string> m_initialProgramPlans;

        /// <summary>
        /// The builders are in charge of assembling the dynamic plan and makes all elements available
        /// A GPPlanBuilder creates one plan and a chromosome for it using the build method.
        /// </summary>
        private Dictionary<string, GPPlanBuilder> m_builders;

        /// <summary>
        /// The dictionary is counting up the generations and each value contains a list of all planNames made up during that new generation.
        /// </summary>
        private Dictionary<int, List<string>> m_generations;

        private int genCounter;
        /// <summary>
        /// resulting plans which are evolved by the GP each having its own unique key
        /// </summary>
        private Dictionary<string, string> m_evolvedPlans;

        /// <summary>
        /// using the planidentifier from evolvedPlans the fit value of a plan is stored in a list one value for each evaluation.
        /// </summary>
        private Dictionary<string, List<float>> m_planFitness;

        private GenoType m_genoType;

        public GPSystem()
        {
            m_randomGenerator = new Random();
            m_initialProgramPlans = new Dictionary<string,string>();
            m_builders = new Dictionary<string, GPPlanBuilder>();
            m_evolvedPlans = new Dictionary<string,string>();
            m_planFitness = new Dictionary<string, List<float>>();
            working = false;


            genCounter = 0;
            m_generations = new Dictionary<int, List<string>>();

        }

        /// <summary>
        /// uses the input plans as a seed for the Gp to come up with similar plans
        /// </summary>
        /// <param name="plans">Contains the name of the plan and the plan file itself</param>
        /// <returns></returns>
        public bool InitGPWithPlan(Tuple<string,string>[] plans)
        {
            if (working)
                return false;
            m_reader = new GPLapReader();

            for (int i = 0; i < plans.Length; i++)
            {
                m_initialProgramPlans.Add(plans[i].First, plans[i].Second);

                m_builders.Add(plans[i].First, m_reader.Parse(plans[i].Second));
            }
            return true;

        }

        public bool Configure(int poolSize, int agentCount, int maxNodes,float mutationRate, float newChromosomeRate,bool verbose)
        {

            m_config = new Configuration();
            m_config.SetPopulationSize(poolSize);
            m_config.SetMutationRate(mutationRate);
            m_config.SetNewChromosomeRate(newChromosomeRate);
            m_config.SetMaximumNodes(maxNodes);
            m_config.SetVerbose(verbose);
            return true;
        }

        public Tuple<string,string>[] GetEvolvedPlans(int number)
        {
            List<Tuple<string, string>> bestPlans = new List<Tuple<string, string>>();

            for (int i = 0; i < number; i++)
            {
                if (m_generations.ContainsKey(genCounter) && m_generations[genCounter] is List<string> && m_generations[genCounter].Count >= number)
                {
                    string planName = m_generations[genCounter][i];
                    bestPlans.Add(new Tuple<string, string>(planName,m_evolvedPlans[planName]));

                }
            }
                return bestPlans.ToArray();
        }

        public void FeedbackFitness(List<Tuple<string, float>> fitness)
        {
            // this needs to be reworked to accomodate for better feedback on the agents performance
            // ideally it would contain tripplets of sensory input and the related fitness of the output
            foreach (Tuple<string, float> elem in fitness)
                m_planFitness.Add(elem.First, null);
        }



        /// <summary>
        /// specific a seed for the usage during all internal random processes.
        /// The seed is affecting the chromosome selection, xover points, mutation and so on.
        /// </summary>
        /// <param name="seed"></param>
        public void SetRandomSeed(int seed)
        {
            m_randomGenerator = new Random(seed);
        }

        /// <summary>
        /// Creates a genotype for a set of agents using the internal plans which were provided by the InitGPWithPlans method
        /// </summary>
        /// <param name="agents"></param>
        public void ConnectAgents(AgentBase[] agents)
        {
            IGenePool pool = m_config.genePool;
            List<AgentBase> completeAgents = new List<AgentBase>();
            List<GPPlanBuilder> completeBuilder = new List<GPPlanBuilder>();
            for (int i = 0; i < agents.Length; i++)
            {
                if ( m_builders.ContainsKey( agents[i].linkedPlanName ) )
                {
                    completeAgents.Add(agents[i]);
                    completeBuilder.Add(m_builders[agents[i].linkedPlanName]);
                    // COMMENT: current assumption for all agents is that they share the same underlying primitives. 
                    // This does not mean that the agents are identical or even have the same appearance, it just means that their underlying api are identical

                    foreach (POSHAction action in m_builders[agents[i].linkedPlanName].actions)
                    {
                        if (!pool.Contains(action))
                            pool.AddGene(action);
                    }
                    foreach (POSHSense sense in m_builders[agents[i].linkedPlanName].senses)
                    {
                        if (!pool.Contains(sense))
                            pool.AddGene(sense);
                    }
                }
                // we need to go through all agents and check their plans for all existing actions to construct an inital set of actions and senses used for the genepool

            }
            m_genoType = GenoType.LoadPOSHGenotype(m_config,completeBuilder.ToArray(),completeAgents.ToArray(),pool);
            //need to work from here on by linking a programchromosome to the agent for getting data on the evaluation
            // the program chromsome needs to implement an Ilistener as well to listen on the agents actions for later eval

        }



        public bool EvaluateAgents()
        {
            throw new NotImplementedException();
        }

        public void Evolve()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
