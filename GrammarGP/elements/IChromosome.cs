using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;

namespace GrammarGP.elements
{
    public interface IChromosome : ICloneable
    {

        //Configuration GetGPConfiguration();
        
        AGene[] GetGenes();

        AGene GetGene(decimal genePos);
        
        /// <summary>
        /// Inserts a gene at a specific position in the chromosome replacing the one at the specific position. 
        /// The same gene can not be already part of the chromosome. If the position is not already taken be a gene, the new gene is inserted at the end of the chromosome.
        /// </summary>
        /// <param name="genePos">The position in the chromosome the new gene should have.</param>
        /// <param name="gene">The gene to insert</param>
        /// <param name="recursive">If true, inserts the gene and all its children at a specific position replacing the gene at that specific position. 
        /// All children of the old gene are removed from the chromosome. If false, the gene is only overriding the gene at the specific position and inheriting all its children. 
        /// If the gene can not inherit the children they are removed from the chromosome. If the gene originally had children in another chromosome they are not moved if recursive is set to false.</param>
        /// <returns>If the gene could be inserted the method returns true, False otherwise.</returns>
        bool InsertGene(decimal genePos, AGene gene, bool recursive);

        decimal GetGenePosition(AGene gene);

        decimal[] GetAllFunctions();

        decimal[] GetAllLeafs();

        /// <summary>
        /// Returns all genes which can fit in a similar place occupied by a specific gene type and returns a matching type as well.
        /// This method encodes POSH plan rules regulating which nodes can fit at which place in a POSH plan.
        /// </summary>
        /// <param name="gType">The AGene Type of the original gene.</param>
        /// <param name="rType">The AGene returnType of the original gene.</param>
        /// /// <param name="leafs">If true, the method only returns genes which are leafs. 
        /// If false, the method returns genes which contain children.</param>
        /// <returns>An array of gene IDs which fit in the same place as the original gene.</returns>
        decimal[] GetAllInterChangeableGenes(AGene.GeneType gType, AGene.ReturnType retType, bool leafs);

        bool AcceptsGenesofType(AGene.GeneType type);
        
        bool AddGene(AGene gene);

        bool RemoveGene(AGene gene);

        bool RemoveGene(decimal genePos);

        bool Contains(AGene gene);

        bool IsLeaf(AGene.GeneType gType);
        
        bool Validate();

        int GetSize();

        int GetSize(decimal node);

        int GetDepth(decimal node);
    }
}
