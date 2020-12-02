using System;
using System.Collections.Generic;
using System.Linq;

namespace Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis
{
    public class ReachingDefinitionConstraints : IConstraints
    {
        // Variable/ArrayName -> (name, edgeStart, edgeEnd)
        public Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>
            VariableToPossibleAssignments;

        public ReachingDefinitionConstraints()
        {
           VariableToPossibleAssignments = new Dictionary<string, HashSet<(string variable, string startNode, string endNode)>>();
        }

        public bool IsSubset(IConstraints other)
        {
            if (!(other is ReachingDefinitionConstraints otherReachingDefinitions))
            {
                return false;
            }

            foreach (var entry in VariableToPossibleAssignments)
            {
                var containsKey =
                    otherReachingDefinitions.VariableToPossibleAssignments.TryGetValue(entry.Key,
                        out var otherConstraintNodeSet);
                if (!containsKey)
                {
                    return false;
                }

                if (entry.Value.Any(tuple => !otherConstraintNodeSet.Contains(tuple)))
                {
                    return false;
                }
            }

            return true;
        }
        
        public IConstraints Clone()
        {
            var clonedConstraints = new ReachingDefinitionConstraints();
            foreach (var variableToPossibleAssignment in VariableToPossibleAssignments)
            {
                clonedConstraints.VariableToPossibleAssignments[variableToPossibleAssignment.Key] =
                    variableToPossibleAssignment.Value;
            }

            return clonedConstraints;
        }
        
        public void Join(IConstraints other)
        {
            if (!(other is ReachingDefinitionConstraints reachingDefinitionConstraints))
            {
                throw new Exception($"Join operator called with constraint type that was different from {nameof(ReachingDefinitionConstraints)}");
            }
            foreach (var (key, set) in reachingDefinitionConstraints.VariableToPossibleAssignments)
            {
                var keyExists = VariableToPossibleAssignments.TryGetValue(key, out var existingSet);
                if (!keyExists)
                {
                    existingSet = new HashSet<(string variable, string startNode, string endNode)>();
                    VariableToPossibleAssignments[key] = existingSet;
                }
                existingSet.UnionWith(set);
            }
        }
        
        public bool IsSuperSet(IConstraints other)
        {
            throw new System.NotImplementedException();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ReachingDefinitionConstraints other))
            {
                return false;
            }

            return VariableToPossibleAssignments.Keys.SequenceEqual(other.VariableToPossibleAssignments.Keys)
                   && VariableToPossibleAssignments.All(kvp =>
                       kvp.Value.SetEquals(other.VariableToPossibleAssignments[kvp.Key]));
        }

        public override int GetHashCode()
        {
            return VariableToPossibleAssignments.GetHashCode(); //Only used in a non readonly fashion in tests
        }
        
        public override string ToString()
        {
            return 
                string.Join(", ", 
                    VariableToPossibleAssignments
                        .Select(entry => $"({entry.Key}, {{{string.Join(", ", entry.Value)}}})"));
        }
    }
}