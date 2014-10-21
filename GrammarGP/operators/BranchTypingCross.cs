using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.elements;
using GrammarGP.env;

namespace GrammarGP.operators
{
    public class BranchTypingCross : ICrossOverOperator
    {
        private Configuration m_config;

        public BranchTypingCross(Configuration config)
        {
            m_config = config;
        }

            private decimal RandomElement(Random random, decimal [] elementIDs)
            {
                decimal foundID = -1;
                // Choose a function.
                // ------------------
                int nf = elementIDs.Length;
                if (nf == 0) 
                {
                    // No functions there.
                    // -------------------
                    foundID = -1;
                }
                  
                int fctIndex = random.Next(nf);
                foundID = elementIDs[fctIndex];
            
                return foundID;

            }

    

        public IChromosome[]  DoCross(IChromosome a_chrom, IChromosome b_chrom)
        {
             IChromosome[] c = { a_chrom, b_chrom};
            
            /* TODO(swen) there is no check if chromosomes are crossed with different arities 
             * which can lead to run-time exceptions and invalid structures */
            Random random = m_config.randomGenerator;


     

            // insertion point for x-over in chrom 1
            decimal ic0;

            if (random.NextDouble() < m_config.functionProbability) {
                // Choose a function.
                // ------------------
                ic0 = RandomElement(random,a_chrom.GetAllFunctions());
                if(ic0 == -1)
                    return c;
            }
            else 
            {
                // Choose a terminal.
                // ------------------
                ic0 = RandomElement(random, a_chrom.GetAllLeafs());
                // Mutate the command's value.
                // ----------------------------
                AGene command = a_chrom.GetGene(ic0);
                if (random.NextDouble() <= m_config.mutationRate && command is AGene) {
                    /* FIXME(swen) added a random mutation rate instead of arbitrary 0.3d */ 
                    command = command.Mutate((float)random.NextDouble());
                    if (command != null) {
                    // Check if mutant's function is allowed.
                    // --------------------------------------
        	        /* FIXME(swen) changed getCommandOfClass to AvailableCommand to use the whole available function set for the chromosome for mutation*/
        	        if (b_chrom.AcceptsGenesofType(command.type) ) 
                        a_chrom.InsertGene(ic0, command);
                    
                    }
                }
                
            }

            // getting all infos on first selected gene
            AGene nodeP0 = a_chrom.GetGene(ic0);
            AGene.ReturnType p0Type = nodeP0.returnType;
            AGene.ReturnType subType = nodeP0.childenReturnType;
                
            // Choose a point in c2 matching the type and subtype of p0.
            // ---------------------------------------------------------

            //insertion point for x-over in chrom 2
            decimal ic1;
                
            if (random.NextDouble() < m_config.functionProbability) {
                  ic1 = RandomElement(random,b_chrom.GetAllInterChangeableGenes(nodeP0.type, p0Type,false));
                }
            else {
              // Choose a terminal.
              // ------------------
                ic1 = RandomElement(random,b_chrom.GetAllInterChangeableGenes(nodeP0.type, p0Type,true));
                if(ic1 == -1)
                    return c;
              // Mutate the command's value.
              // ----------------------------
              AGene command = b_chrom.GetGene(ic1);
              if (random.NextDouble() <= m_config.mutationRate && command is AGene) {
                  command = command.Mutate((float) random.NextDouble());
                  if (command != null) {
                    // Check if mutant's function is allowed.
                    // --------------------------------------
                      /* FIXME(swen) changed getCommandOfClass to AvailableCommand to use the whole available function set for the chromosome for mutation*/
        	        if (b_chrom.AcceptsGenesofType(command.type) ) 
                        b_chrom.InsertGene(ic1, command);
                  }
              }
            }

            // TODO: solve in general
            if ( !b_chrom.IsLeaf(b_chrom.GetGene(ic1).type) ) {
                b_chrom.InsertGene(ic1, b_chrom.GetGene(ic1).Mutate((float) random.NextDouble()));
            }
            int s0 = a_chrom.GetSize(ic0); //Number of nodes in a_chrom of gene ic0
            int s1 = b_chrom.GetSize(ic1); //Number of nodes in b_chrom of gene ic1
            int d0 = a_chrom.GetDepth(ic0); //Depth of ic0
            int d1 = b_chrom.GetDepth(ic1); //Depth of ic1
            int c0s = a_chrom.GetSize(); //Number of nodes in a_chrom
            int c1s = b_chrom.GetSize(); //Number of nodes in b_chrom
            
            // Check for depth constraint for p1 inserted into a_chrom.
            // ---------------------------------------------------
            if (d0 - 1 + d1/*s1*/ > m_config.maxCrossOverDepth
                || c0s - s0 <= 0
   //             || ic0 + s1 + c0s - ic0 - s0 >= a_chrom.GetAllFunctions().Length
                // corrected to : ic1 + s1 + c0s - ic0 - s0 >= a_chrom.GetAllFunctions().Length
                // changed to : ic1 + s1 + c0s >= a_chrom.GetAllFunctions().Length + ic0 + s0
                // I do not get this formula it originally was the position of your subtree + its size
                ) 
            {
              // Choose the other parent.
              // ------------------------
              c[0] = b_chrom;
            }
            else {
              c[0] = new Chromosome(m_config,
                                           a_chrom.GetAllFunctions().Length,
                                           c[0].getFunctionSet(),
                                           c[0].getArgTypes(),
                                           a_chrom.getIndividual());
              System.arraycopy(a_chrom.getFunctions(), 0, c[0].getFunctions(), 0, p0);
              System.arraycopy(b_chrom.getFunctions(), p1, c[0].getFunctions(), p0, s1);
              System.arraycopy(a_chrom.getFunctions(), p0 + s0, c[0].getFunctions(),
                               p0 + s1, c0s - p0 - s0);
              c[0].redepth();
            }
            
            // Check for depth constraint for p0 inserted into c1.
            // ---------------------------------------------------
            if (d1 - 1 + d0/*s0*/ > m_config.maxCrossOverDepth
                || c1s - p1 - s1 < 0
                || p1 + s0 + c1s - p1 - s1 >= b_chrom.getFunctions().length) {
              // Choose the other parent.
              // ------------------------
              c[1] = a_chrom;
            }
            else {
              c[1] = new ProgramChromosome(m_config,
                                           b_chrom.getFunctions().length,
                                           c[1].getFunctionSet(),
                                           c[1].getArgTypes(),
                                           b_chrom.getIndividual());
              System.arraycopy(b_chrom.getFunctions(), 0, c[1].getFunctions(), 0, p1);
              System.arraycopy(a_chrom.getFunctions(), p0, c[1].getFunctions(), p1, s0);
              System.arraycopy(b_chrom.getFunctions(), p1 + s1, c[1].getFunctions(),
                               p1 + s0, c1s - p1 - s1);
              c[1].redepth();
            }
            return c;
        }


    }
}
