using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;

namespace GrammarGP.elements
{
    class Chromosome : IChromosome,ICloneable
    {
        private Dictionary<decimal,AGene> m_genes;
        private Configuration m_config; 
        private decimal geneCounter;

        /// <summary>
        /// Creates a Chromosome using an AGene as a treeNode and extracting all Genes from it into the Chromosome
        /// </summary>
        /// <param name="config">the GP Configuration</param>
        /// <param name="treeRoot">The topmost node of the tree prepresentation to use for the chromosome</param>
        public Chromosome(AGene treeRoot)
            : this ()
        {
            m_config = treeRoot.gpConfig;
        }

        public Chromosome()
        {
            m_genes = new Dictionary<decimal, AGene>();
          //  m_gpConfig = config;
            geneCounter = 0;
        }
    
        public object  Clone()
        {
            Chromosome clone = new Chromosome();
            List<AGene> clonedGenes = new List<AGene>();
            foreach (AGene gene in m_genes.Values)
                clonedGenes.Add((AGene)gene.Clone());

 	        throw new NotImplementedException();
        }



        public AGene[] GetGenes()
        {
            return m_genes.Values.ToArray();
        }

        /// <summary>
        /// Adds a new gene to the chromosome.
        /// If the Gene is already part of the chromosome, the gene will not be added and the method returns false.
        /// </summary>
        /// <param name="gene">A single gene which should be added to the chromosome</param>
        /// <returns>True if the gene was successfully added, False if the gene is already a part of the chromosome.</returns>
        public bool AddGene(AGene gene)
        {
            if (Contains(gene))
                return false;

            gene.id = geneCounter;
            gene.m_Chromosome = this;
            m_genes.Add(geneCounter++, gene);
            return true;
        }

        public bool Validate()
        {
            foreach (AGene child in m_genes.Values)
                if (!child.Validate())
                    return false;
        
            return true;
        }


        public AGene GetGene(decimal genePos)
        {
            return m_genes.ContainsKey(genePos) ? m_genes[genePos] : null;
        }

        public bool InsertGene(decimal genePos, AGene gene)
        {   
            if (Contains(gene))
                return false;
            //TODO: depending on the rest of the implementation I am not sure if only putting a gene at a chrom position is good because this will break the gene and gene-child relation. 
            // On the other side inserting genes into a chromosome at a specific position is useful for evolution of the chrom and is not intended if you just want to add a gene
            if (geneCounter <= genePos)
            {
                gene.id = geneCounter;
                m_genes[geneCounter++] = gene;
            }
            else
            {
                m_genes[genePos] = gene;
                gene.id = genePos;
            }
            gene.m_Chromosome = this;

            return true;
        }

        public decimal GetGenePosition(AGene gene)
        {
            if (m_genes.ContainsKey(gene.id) && m_genes[gene.id].Equals(gene))
                return gene.id;

            foreach (AGene child in m_genes.Values)
                if (child.Equals(gene))
                    return child.id;
           
            return -1;
        }

        public bool RemoveGene(AGene gene)
        {
            decimal pos = GetGenePosition(gene);
            if (pos != -1)
                RemoveGene(pos);
            return false;
        }

        public bool RemoveGene(decimal genePos)
        {
            m_genes[genePos].m_Chromosome = null;
            return m_genes.Remove(genePos);
        }

        public bool Contains(AGene gene)
        {
            if (GetGenePosition(gene) != -1)
                return true;
            return false;
        }

        public decimal[] GetAllFunctions()
        {
            return GetGenes(false);
        }

        public decimal[] GetAllLeafs()
        {
            return GetGenes(true);
        }

        public bool AcceptsGenesofType(AGene.GeneType type)
        {
            return (m_config.genePool.GetAllGenes(type).Length > 0) ? true : false;
        }

        public decimal [] GetGenes(bool leafs)
        {
            List<decimal> genes = new List<decimal>();

            foreach (KeyValuePair<decimal,AGene> gene in m_genes)
                if (IsLeaf(gene.Value.type) == leafs)
                    genes.Add(gene.Key);
                

            return genes.ToArray();
        }


        public bool IsLeaf(AGene.GeneType gType)
        {
            switch (gType)
            {
                case AGene.GeneType.Action:
                case AGene.GeneType.Terminal:
                case AGene.GeneType.Predicate:
                case AGene.GeneType.None:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns all genes which can fit in a similar place occupied by a specific gene type and returns a matching type as well.
        /// This method encodes POSH plan rules regulating which nodes can fit at which place in a POSH plan.
        /// </summary>
        /// <param name="gType">The AGene Type of the original gene.</param>
        /// <param name="rType">The AGene returnType of the original gene.</param>
        /// /// <param name="leafs">If true, the method only returns genes which are leafs. 
        /// If false, the method returns genes which contain children.</param>
        /// <returns>An array of gene IDs which fit in the same place as the original gene.</returns>
        public decimal[] GetAllInterChangeableGenes(AGene.GeneType gType, AGene.ReturnType retType, bool leafs)
        {
            List<decimal> genes = new List<decimal>();

            foreach (KeyValuePair<decimal, AGene> gene in m_genes)
                if ((gType == gene.Value.type && retType == gene.Value.returnType) || gene.Value.InterchangeableWith(gType, retType))
                    if (IsLeaf(gene.Value.type) == leafs)
                        genes.Add(gene.Key);


            return genes.ToArray();
        }

        public int GetSize()
        {
            return m_genes.Count;
        }

        public int GetSize(decimal node)
        {
            // FIXME: this is definetly not a good approach when having a linear representation to parse the tree but it will need to get fixed later
            int size = 1;
            AGene gene = m_genes[node];
            if (IsLeaf(gene.type))
                return size;
            foreach (decimal childID in gene.children)
                size += GetSize(childID);

            return size;
        }

        public int GetDepth(decimal node)
        {
 
        }
    }
}
