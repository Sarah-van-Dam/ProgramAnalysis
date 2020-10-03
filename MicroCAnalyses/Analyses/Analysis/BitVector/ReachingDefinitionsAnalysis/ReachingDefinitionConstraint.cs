using System.Collections.Generic;

namespace Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis
{
    public class ReachingDefinitionConstraints : Constraints<(string variable, string startNode, string endNode)>
    {
        public readonly Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
            VariableToPossibleAssignments;

        public ReachingDefinitionConstraints()
        {
           VariableToPossibleAssignments = 
               new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>();
        }


        public override bool IsSubset(Constraints<(string variable, string startNode, string endNode)> other)
        {
            throw new System.NotImplementedException();
        }
    }
}