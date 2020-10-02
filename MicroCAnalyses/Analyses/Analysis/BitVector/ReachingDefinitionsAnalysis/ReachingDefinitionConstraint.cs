using System.Collections.Generic;
using System.Linq;

namespace Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis
{
    public class ReachingDefinitionConstraints : Constraints
    {
        public readonly Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
            VariableToPossibleAssignments;

        public ReachingDefinitionConstraints()
        {
           VariableToPossibleAssignments = new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>();
        }
    }
}