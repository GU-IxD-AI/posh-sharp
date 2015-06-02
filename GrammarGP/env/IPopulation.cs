using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.operators;
using GrammarGP.elements;

namespace GrammarGP.env
{
	public interface IPopulation : IComparable
    {

		/// <summary>
		/// Sorts the population into "ascending" order using some criterion for
		/// "ascending". An Evaluator is given which will compare two individuals,
		/// and if one individual compares lower than another individual, the first
		/// individual will appear in the population before the second individual.
		/// </summary>
		/// <param name="c">An IFitnessEvaluator used for comparing the fitness of two programs</param>
		void Sort (IFitnessEvaluator c);


		/**
   * Creates a population.
   *
   * @param a_types the type for each chromosome, the length of the array
   * represents the number of chromosomes
   * @param a_argTypes the types of the arguments to each chromosome, must be an
   * array of arrays, the first dimension of which is the number of chromosomes
   * and the second dimension of which is the number of arguments to the
   * chromosome
   * @param a_nodeSets the nodes which are allowed to be used by each chromosome,
   * must be an array of arrays, the first dimension of which is the number of
   * chromosomes and the second dimension of which is the number of nodes
   * @param a_minDepths contains the minimum depth allowed for each chromosome
   * @param a_maxDepths contains the maximum depth allowed for each chromosome
   * @param a_maxNodes reserve space for a_maxNodes number of nodes
   * @param a_fullModeAllowed array of boolean values. For each chromosome there
   * is one value indicating whether the full mode for creating chromosome
   * generations during evolution is allowed (true) or not (false)
   * @param a_programCreator service to create new programs with
   * @throws InvalidConfigurationException
   *
   * @author Klaus Meffert
   * @since 3.2.2
   */

		void Create(Type[] types, Type[][] argTypes,
			AGene[][] nodeSets, int[] minDepths,
			int[] maxDepths, int maxNodes,
			bool[] fullModeAllowed,
			IProgramCreator programCreator);

		/**
   * Creates a population.
   *
   * @param a_types the type for each chromosome, the length of the array
   * represents the number of chromosomes
   * @param a_argTypes the types of the arguments to each chromosome, must be an
   * array of arrays, the first dimension of which is the number of chromosomes
   * and the second dimension of which is the number of arguments to the
   * chromosome
   * @param a_nodeSets the nodes which are allowed to be used by each chromosome,
   * must be an array of arrays, the first dimension of which is the number of
   * chromosomes and the second dimension of which is the number of nodes
   * @param a_minDepths contains the minimum depth allowed for each chromosome
   * @param a_maxDepths contains the maximum depth allowed for each chromosome
   * @param a_maxNodes reserve space for a_maxNodes number of nodes
   * @param a_fullModeAllowed array of boolean values. For each chromosome there
   * is one value indicating whether the full mode for creating chromosome
   * generations during evolution is allowed (true) or not (false)
   * @param a_programCreator service to create new programs with
   * @param a_offset start index for new programs to put into the configuration
   *
   * @throws InvalidConfigurationException
   *
   * @author Klaus Meffert
   * @since 3.3.3
   */
		public void create(Class[] a_types, Class[][] a_argTypes,
			CommandGene[][] a_nodeSets, int[] a_minDepths,
			int[] a_maxDepths, int a_maxNodes,
			boolean[] a_fullModeAllowed,
			IProgramCreator a_programCreator,
			int a_offset);

		/**
   * Creates a population.
   *
   * @param a_types Class[]
   * @param a_argTypes Class[][]
   * @param a_nodeSets CommandGene[][]
   * @param a_minDepths int[]
   * @param a_maxDepths int[]
   * @param a_depth int
   * @param a_grow boolean
   * @param a_maxNodes int
   * @param a_fullModeAllowed boolean[]
   * @param a_tries int
   * @return IGPProgram
   * @throws InvalidConfigurationException
   *
   * @author Klaus Meffert
   * @since 3.2.2
   */
		public IGPProgram create(Class[] a_types, Class[][] a_argTypes,
			CommandGene[][] a_nodeSets, int[] a_minDepths,
			int[] a_maxDepths, int a_depth, boolean a_grow,
			int a_maxNodes, boolean[] a_fullModeAllowed,
			int a_tries);

		/**
   *
   * @param a_programIndex int
   * @param a_types Class[]
   * @param a_argTypes Class[][]
   * @param a_nodeSets CommandGene[][]
   * @param a_minDepths int[]
   * @param a_maxDepths int[]
   * @param a_depth int
   * @param a_grow boolean
   * @param a_maxNodes int
   * @param a_fullModeAllowed boolean[]
   * @param a_tries int
   * @return IGPProgram
   * @throws InvalidConfigurationException
   *
   * @author Klaus Meffert
   * @since 3.3
   */
		public IGPProgram create(int a_programIndex, Class[] a_types,
			Class[][] a_argTypes,
			CommandGene[][] a_nodeSets, int[] a_minDepths,
			int[] a_maxDepths, int a_depth, boolean a_grow,
			int a_maxNodes, boolean[] a_fullModeAllowed,
			int a_tries);

		/**
   * Creates a valid IGPProgram. No fitness computation is initiated here!
   *
   * @param a_programIndex index of the program in the population
   * @param a_types the type of each chromosome, the length is the number of
   * chromosomes
   * @param a_argTypes the types of the arguments to each chromosome, must be an
   * array of arrays, the first dimension of which is the number of chromosomes
   * and the second dimension of which is the number of arguments to the
   * chromosome
   * @param a_nodeSets the nodes which are allowed to be used by each chromosome,
   * must be an array of arrays, the first dimension of which is the number of
   * chromosomes and the second dimension of which is the number of nodes
   * @param a_minDepths contains the minimum depth allowed for each chromosome
   * @param a_maxDepths contains the maximum depth allowed for each chromosome
   * @param a_depth the maximum depth of the program to create
   * @param a_grow true: grow mode, false: full mode
   * @param a_maxNodes reserve space for a_maxNodes number of nodes
   * @param a_fullModeAllowed array of boolean values. For each chromosome there
   * is one value indicating whether the full mode for creating chromosomes
   * during evolution is allowed (true) or not (false)
   * @param a_tries maximum number of tries to get a valid program
   * @param a_programCreator strategy class to create programs for the
   * population
   *
   * @return valid program
   *
   * @throws InvalidConfigurationException
   *
   * @author Klaus Meffert
   * @since 3.0
   */
		public IGPProgram create(int a_programIndex, Class[] a_types,
			Class[][] a_argTypes,
			CommandGene[][] a_nodeSets, int[] a_minDepths,
			int[] a_maxDepths, int a_depth, boolean a_grow,
			int a_maxNodes, boolean[] a_fullModeAllowed,
			int a_tries, IProgramCreator a_programCreator);
			
		/// <summary> Returns the fixed size of the population</summary>
		/// <returns> the fixed maximal size of the population</returns>
		int GetFixedPopSize();

		Configuration GetGPConfiguration();

		/// <summary>
		/// Sets an individual program in the population at the location of the index. Overriding the program at that location
		/// </summary>
		/// <param name="index"> the index of within the population</param>
		/// <param name="program">the program to put at the location of index</param>
		void SetProgram(final int index, IProgram program);

		/// <summary>
		/// Sets the porgrams of the population to the given programs and overrides all contained programs
		/// </summary>
		void SetPrograms( IProgram[] progs);

		/// <summary>
		/// Sets the porgrams of the population to the given programs and overrides all contained programs
		/// </summary>
		void SetPrograms(ICollection pop);

		/// <summary>
		/// Returns an individual program form the population at the location of the index
		/// </summary>
		/// <param name="index"> the index of a given program in the population</param>
		IProgram GetProgram(int index);

		/// <summary>
		/// Returns an array of all contained programs
		/// </summary>
		/// <returns> array containing all programs of the population</returns>
		IProgram[] GetPrograms();

		/// <summary>
		/// Calculates the size of the population by the number of programs contained
		/// </summary>
		/// <returns> number of all programs contained in the population</returns>
		int Size();


		/// <summary>
		/// Determines the fittest GPProgram in the population, but only considers
		/// programs with already computed fitness value.
		/// </summary>
		/// <returns> fittest program in the population, where the fitness has been computed during the last evaluation</returns>
		IProgram DetermineFittestProgramComputed();


		/// <summary>
		/// Sorts the GPPrograms list and returns the fittest n GPPrograms in the population.
		/// </summary>
		/// <param name="numberOfPrograms"> number of programs to return </param>
		/// <returns>array of the fittest n GPPrograms of the population, or the fittest
		/// IPrograms with x = number of GPPrograms in case n > x.</returns>
		IProgram[] DetermineFittestPrograms(int a_numberOfPrograms);

		/// <summary>
		/// Sorts the programs within the population according to their fitness
		/// value.
		/// </summary>
		void SortByFitness();

		float[] GetFitnessRanks();

		float GetFitnessRank(int a_index);


		/// <summary>
		/// checks if the population has changed
		/// value.
		/// </summary>
		/// <returns>true: population's programs (maybe) were changed, false: not changed for sure</returns>
		bool IsChanged();

		/// <summary>
		/// Sets the fittest program of the population
		/// value.
		/// </summary>
		/// <param name="toAdd">the program to add</param>
		void AddFittestProgram(final IProgram toAdd);


		/// <summary>
		/// Clears the population of all programs.
		/// value.
		/// </summary>
		void Clear();

		/// <summary>
		/// 
		/// 
		/// </summary>
		/// <returns>persistent representation of the polupation</returns>
		string GetPersistentRepresentation();
	

    }
}
