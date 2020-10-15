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

        public bool IsSubset(IConstraints other)
        {
            if (!(other is ReachingDefinitionConstraints))
            {
                return false;
            }

            var otherReachingDefinitions = (ReachingDefinitionConstraints) other;
            foreach (var entry in VariableToPossibleAssignments)
            {
                var containsKey =
                    otherReachingDefinitions.VariableToPossibleAssignments.TryGetValue(entry.Key,
                        out var otherConstraintNodeSet);
                if (!containsKey)
                {
                    return false;
                }

                foreach (var tuple in entry.Value)
                {
                    if (!otherConstraintNodeSet.Contains(tuple))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}