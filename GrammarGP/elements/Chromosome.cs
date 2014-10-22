using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;

namespace GrammarGP.elements
{
    class Chromosome : IChromosome
    {
        private Dictionary<decimal,AGene> m_genes;
        private Configuration m_config; 
        private decimal geneCounter;
        private AGene m_rootNode;

        /// <summary>
        /// Creates a Chromosome using an AGene as a treeNode and extracting all Genes from it into the Chromosome
        /// </summary>
        /// <param name="config">the GP Configuration</param>
        /// <param name="treeRoot">The topmost node of the tree prepresentation to use for the chromosome</param>
        public Chromosome(AGene treeRoot)
            : this(treeRoot.gpConfig)
        {
            m_rootNode = treeRoot;
        }

        public Chromosome(Configuration config)
        {
            m_genes = new Dictionary<decimal, AGene>();
            m_config = config;
            geneCounter = 0;
        }
    
        /// <summary>
        /// Used during Crossover to create a deep clone of the chromosome including all genes
        /// </summary>
        /// <returns>a new chromsome based on original one</returns>
        public object  Clone()
        {
            Chromosome clone = new Chromosome(m_config);
            List<AGene> clonedGenes = new List<AGene>();
            foreach (AGene gene in m_genes.Values)
                clonedGenes.Add((AGene)gene.DeepClone(clone));

            return clone;
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

        public bool InsertGene(decimal genePos, AGene gene, bool recursive)
        {
            if (!recursive)
                gene.ReleaseGene();
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
                if (m_genes[genePos].InterchangeableWith(gene.type,gene.returnType))
                    ReplaceGene(genePos,gene);
                else 
                    return false;
            
            }
            gene.m_Chromosome = this;

            return true;
        }

        private void ReplaceGene(decimal pos, AGene newGene)
        {
            AGene oldGene = m_genes[pos];
            List<AGene> oldChildren = new List<AGene>();
            
            foreach (decimal childID in oldGene.children)
                oldChildren.Add(m_genes[childID]);

            if (!newGene.SetChildren(oldChildren.ToArray()))
                RemoveGene(oldGene);
            m_genes[pos] = newGene;
            newGene.id = pos;
        
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

            if (m_genes.ContainsKey(pos))
                return RemoveGene(pos);

            return false;
        }

        public bool RemoveGene(decimal genePos)
        {
            m_genes[genePos].m_Chromosome = null;
            if (!m_genes.ContainsKey(genePos))
                return false;
            
            foreach (decimal child in m_genes[genePos].children)
                RemoveGene(child);

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
            // FIXME: this is definetly not a good approach when having a linear representation to parse the tree but it will need to get fixed later
            int size = 0;
            AGene gene = m_genes[node];
            if (IsLeaf(gene.type))
                return size;
            foreach (decimal childID in gene.children)
                size += GetDepth(childID,0);

            return size;
        }

        private int GetDepth(decimal node,int parentDepth)
        {
            // FIXME: this is definetly not a good approach when having a linear representation to parse the tree but it will need to get fixed later
            int depth = parentDepth+1;
            int maxChildDepth = 0;

            AGene gene = m_genes[node];
            if (IsLeaf(gene.type))
                return depth;
            foreach (decimal childID in gene.children)
            {
                int d = GetDepth(childID,depth);
                if (d > maxChildDepth)
                    maxChildDepth = d;
            }
            return maxChildDepth;
        }
    }
}
