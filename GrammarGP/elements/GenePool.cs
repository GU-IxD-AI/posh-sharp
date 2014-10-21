using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarGP.elements
{
    public class GenePool : IGenePool
    {
        protected Dictionary<AGene.GeneType, List<AGene>> genes;
        private decimal geneCounter = 0;

        public GenePool()
        {
            genes = new Dictionary<AGene.GeneType, List<AGene>>();
        }

        public bool AddGene(AGene gene)
        {
            if (Contains(gene))
                return false;

            if (!genes.ContainsKey(gene.type))
                genes.Add(gene.type, new List<AGene>());
            genes[gene.type].Add(gene);
            gene.id = GenerateId();

            return false;
        }

        public bool RemoveGene(AGene gene)
        {
            return (Contains(gene)) ? genes[gene.type].Remove(gene): false;
        }

        public bool RemoveGene(decimal geneID)
        {
            return (RemoveGene(GetGene(geneID)));
        }

        public AGene[] GetAllGenes(AGene.GeneType gType)
        {
            return (genes.ContainsKey(gType)) ? genes[gType].ToArray() : new AGene[0];
        }

        public AGene GetGene(decimal geneID)
        {
            foreach (List<AGene> list in genes.Values)
                foreach (AGene gene in list)
                    if (gene.id == geneID)
                        return gene;
            
            return null;
        }

        public bool Contains(AGene gene)
        {
            if (gene is AGene && genes.ContainsKey(gene.type) && genes[gene.type].Contains(gene))
                return true;

            return false;
        }

        private decimal GenerateId()
        {
            return geneCounter++;
        }


        public object Clone()
        {
            GenePool clone = new GenePool();
            clone.geneCounter = geneCounter;

            //TODO: the gene dict should be a deep copy as well but right now this will do fine
            clone.genes = genes;

            return clone;
        }
    }
}
