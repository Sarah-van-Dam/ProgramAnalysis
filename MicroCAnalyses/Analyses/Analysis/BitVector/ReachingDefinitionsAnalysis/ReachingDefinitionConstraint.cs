using System.Collections.Generic;
using Analyses.Graph;

namespace Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis
{
    public class ReachingDefinitionConstraints : IConstraints
    {
        public readonly Dictionary<string, HashSet<(string action, string startNode, string endNode)>>
            variableToPossibleAssignments;

        public ReachingDefinitionConstraints()
        {
           variableToPossibleAssignments = new Dictionary<string, HashSet<(string action, string startNode, string endNode)>>();
        }
        

    }
}