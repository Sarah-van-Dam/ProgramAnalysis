using System.Collections.Generic;

namespace Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis
{
    public class ReachingDefinitionConstraints : Constraints
    {
        public readonly Dictionary<string, HashSet<(string action, string startNode, string endNode)>>
            VariableToPossibleAssignments;

        public ReachingDefinitionConstraints()
        {
           VariableToPossibleAssignments = new Dictionary<string, HashSet<(string action, string startNode, string endNode)>>();
        }
        

    }
}