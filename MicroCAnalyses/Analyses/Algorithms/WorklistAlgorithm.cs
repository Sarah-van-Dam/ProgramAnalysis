using Analyses.Graph;

namespace Analyses.Algorithms
{
    public abstract class WorklistAlgorithm
    {
        public int BasicActionsNeeded { get; protected set; }
        
        public abstract bool Empty();

        public abstract void Insert(Node q);

        public abstract Node Extract();
    }
}