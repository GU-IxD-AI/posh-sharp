using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GrammarGP.env;
using GrammarGP.elements.exceptions;
using POSH.sys;

namespace GrammarGP.elements
{
    public abstract class AGene : ICloneable
    {

        public enum GeneType
        {
            None, Terminal, Predicate, Action, Sense, Goal,
            ActionPattern, CompetenceElement, CompetencePriorityElement, Competence,
            DriveElement, DrivePriorityElement, DriveCollection
        }

        public enum ReturnType { Void, Bool, Number, Text }

        public GeneType type { get; protected set; }
        public ReturnType returnType { get; protected set; }

        public decimal parent;
        public decimal id { get; protected internal set; }

        /// <summary>
        /// contains the slot ids for the children of this gene. The ids are based in the same chromosome as the parent gene.
        /// </summary>
        public List<decimal> children { get; protected set; }
        public List<GeneType> childTypes { get; protected set; }
        
        protected internal IChromosome m_Chromosome { get; internal set; }

        /// <summary>
        /// Specifies the returntype of all children of this gene.
        /// If the type is null is means that the children can return different things.
        /// </summary>
        public ReturnType childenReturnType { get; protected set; }

        
        public object value { get; protected set; }

        public Configuration gpConfig;

        public AGene(Configuration config, GeneType geneType, ReturnType rType)
        {
            gpConfig = config;
            type = geneType;
            returnType = rType;
            parent = -1;
            childTypes = new List<GeneType>();
            children = new List<decimal>();
        }

        public AGene(Configuration config, GeneType geneType, ReturnType rType, object value)
            : this(config,geneType,rType)
        {
            this.value = value;
        }

        /// <summary>
        /// Sets all children of a gene. The children are added to the same chromosome the parent is part of
        /// </summary>
        /// <param name="children"></param>
        /// <returns></returns>
        public virtual bool SetChildren(AGene [] children)
        {   
            // not sure if it would be OK to just insert all possible instead of forcing an 
            // actual match in the number of children and types relating to them
            if (children.Length != childTypes.Count && m_Chromosome == null)
                return false;

            this.children.Clear();
            
            List<AGene> childList = new List<AGene>();

            for(int i = 0; i < children.Length; i++)
            {
                if (childTypes[i] != children[i].type 
                    || (childenReturnType != ReturnType.Void && childenReturnType != children[i].returnType))
                    throw new GPTypeMismatchException();
                childList.Add(children[i]);
            }

            foreach (AGene elem in childList)
            {
                if (m_Chromosome.AddGene(elem))
                {
                    elem.parent = this.id;
                    this.children.Add(elem.id);
                }
                else if (m_Chromosome == elem.m_Chromosome)
                {
                    elem.parent = this.id;
                    this.children.Add(elem.id);
                }
                
            }

            return true;
        }
        
        public virtual bool AddRecursiveToChromosome(IChromosome chrom)
        {
            IChromosome oldChromosome = m_Chromosome;
            decimal[] children = this.children.ToArray();

            if (chrom.AddGene(this))
            {
                
                for(int i = 0; i<children.Length;i++) 
                {
                    AGene child = oldChromosome.GetGene(children[i]);
                    if (child is AGene)
                    {
                        if (!child.AddRecursiveToChromosome(chrom))
                            return false;
                        child.parent = this.id;
                        children[i] = child.id;
                    }
                    
                }

                this.children = new List<decimal>(children);
                return true;
            } 
            
            return false;
        }

        public abstract object Clone();
        
        public virtual object DeepClone(IChromosome targetChrom)
        {
            AGene clone = (AGene)this.Clone();
            clone.returnType = this.returnType;
            clone.childenReturnType = this.childenReturnType;
            clone.childTypes = new List<GeneType>(this.childTypes.ToArray());

            IChromosome oldChromosome = m_Chromosome;
            decimal[] children = this.children.ToArray();

            if (targetChrom.AddGene(clone))
            {

                for (int i = 0; i < children.Length; i++)
                {
                    AGene child = oldChromosome.GetGene(children[i]);
                    if (child is AGene)
                    {
                        AGene childClone = (AGene)child.DeepClone(targetChrom);
                        childClone.parent = clone.id;
                        children[i] = childClone.id;
                    }

                }

                clone.children = new List<decimal>(children);
                return clone;
            }

            return null;
            
        }

        public abstract override string ToString();
        

        public abstract string ToSerialize(Dictionary<string, string> elements);
        

        /// <summary>
        /// Mutates the gene called upon using the mutation factor to determine how strong the mutation is
        /// </summary>
        /// <param name="mutation">the parameter is a percentage in the range [0f,1.0f] and is expected 
        /// to be within an evenly distributed space (normal distribution)</param>
        public virtual AGene Mutate(float mutation)
        {
            // actions are represented as basic strings so mutating them is not possible at this level 
            // if actions should be changed xover is needed
            List<decimal> existing = new List<decimal>();
            AGene[] pool = new AGene[0];

            //if (mutation < 0.75f)
            // TODO: we could modify the children and delete/add or rearrange them with a low percentage which you correlate well with mutation
            existing.AddRange(m_Chromosome.GetAllInterChangeableGenes(type, returnType, false));

            int pick = (int)MutateNumber(mutation, new Tuple<double, double>(0, existing.Count));

            return (AGene)m_Chromosome.GetGene(existing[pick]).Clone();
        }
        


        public bool Grow(int depth)
        {
            // TODO: this needs to be made absract and implemented in all genes to build up a new tree subtree upon creation
            //depth in this case is identical to breadth I think

            // TODO: There may be a need to split out sopme functionality into a method which validates a subtree on being correct
            throw new NotImplementedException();
        }

        /// <summary>
        /// Validates the Gene and if all children are set correctly and are part of the same Chromosome
        /// </summary>
        /// <returns></returns>
        public virtual bool Validate()
        {
            if (childTypes.Count != children.Count)
                return false;
    
            if (children.Count > 0)
                for (int i = 0; i < childTypes.Count; i++)
                {
                    AGene child = m_Chromosome.GetGene(children[i]);
                    if (child.type != childTypes[i] ||
                        !child.Validate())
                        return false;
                }

            return true;
        }

        protected bool SetChromosome(IChromosome chrom)
        {
            m_Chromosome = (chrom is IChromosome) ? chrom : null;
            m_Chromosome.AddGene(this);
            return (m_Chromosome != null) ? true : false;
        }

        /// <summary>
        /// InterchangeableWith checks if two genes can be exchanged in the chromosome without breaking the underlying logical structure/grammar.
        /// 
        /// </summary>
        /// <param name="gType">The gene type of the gene to compare to.</param>
        /// <param name="retType">The return type of the gene to compare to.</param>
        /// <returns>If types and return types are identical it will return true. If types are compatible for interchange the method returns true, false otherwise.</returns>
        internal virtual bool InterchangeableWith(GeneType gType, ReturnType retType)
        {
            // the same kind of gene is always interchangeable
            if (type == gType && returnType == retType)
                return true;
            // TODO: need to adjust the POSH genes
            // there is still one major issue with the intercahgeable check
            // priority elements and elements in general should be intercahgeable between drive and competence as they theoretically are, yet it needs a bit more more as they need to convert into each other
            switch (type)
            {
                case GeneType.Action:
                case GeneType.ActionPattern:
                case GeneType.Competence:
                    switch (gType)
                    {
                        case GeneType.Action:
                        case GeneType.ActionPattern:
                        case GeneType.Competence:
                            return true;
                        default:
                            return false;
                    }
                default:
                    return false;
            }
        }

        /// <summary>
        /// Removes the gene and all its children from the Chromsome. 
        /// </summary>
        /// <returns></returns>
        internal virtual bool ReleaseGene()
        {
            return m_Chromosome.RemoveGene(this);
        }

        protected double MutateNumber(float mutation, Tuple<double, double> range)
        {
            bool increaseValue = false;

            if (mutation < 0.5f)
                //decrease number
                increaseValue = false;
            else
            {
                //increase number
                increaseValue = true;
                mutation = mutation - 0.5f;
            }

            double number = (double)value;
            double mutationRange = mutation * 2 * (range.Second - range.First);

            if (increaseValue)
                number = (number + mutationRange > range.Second) ? (number + mutationRange) - range.Second + range.First : number + mutationRange;
            else
                number = (number - mutationRange < range.First) ? range.Second - Math.Abs(range.First - Math.Abs(number - mutationRange)) : number - mutationRange;

            return number;
        }
    }
}
