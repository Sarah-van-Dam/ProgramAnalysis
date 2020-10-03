using System.Collections.Generic;

namespace Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis
{
    public class ReachingDefinitionConstraints : IConstraints
    {
        public readonly Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
            VariableToPossibleAssignments;

        public ReachingDefinitionConstraints()
        {
           VariableToPossibleAssignments = new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>();
        }

        public virtual bool IsSubset(IConstraints other)
        {
            throw new System.NotImplementedException();
        }
    }
}