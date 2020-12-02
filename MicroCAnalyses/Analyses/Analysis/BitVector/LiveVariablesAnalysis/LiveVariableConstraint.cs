using System;
using System.Collections.Generic;
using System.Linq;

namespace Analyses.Analysis.BitVector.LiveVariablesAnalysis
{
    public class LiveVariableConstraint : IConstraints
    {
        public HashSet<string> LiveVariables { get; set; }

        public LiveVariableConstraint()
        {
            LiveVariables = new HashSet<string>();
        }

        public bool IsSubset(IConstraints other)
        {
            if (!(other is LiveVariableConstraint otherLiveVariableConstraint))
            {
                return false;
            }
            
            return LiveVariables.All(l => otherLiveVariableConstraint.LiveVariables.Contains(l));
        }

        public bool IsSuperSet(IConstraints other)
        {
            throw new System.NotImplementedException();
        }

        public IConstraints Clone()
        {
            var clonedConstraints = new LiveVariableConstraint();
            foreach (var variable in LiveVariables)
            {
                clonedConstraints.LiveVariables.Add(variable);
            }

            return clonedConstraints;
        }

        public void Join(IConstraints other)
        {
            if (!(other is LiveVariableConstraint liveVariableConstraint))
            {
                throw new Exception($"Join operator called with constraint type that was different from {nameof(LiveVariableConstraint)}");
            }
            LiveVariables.UnionWith(liveVariableConstraint.LiveVariables);
        }
        
        public override bool Equals(object obj)
        {
             if (obj == null || !(obj is LiveVariableConstraint other))
             {
                 return false;
             }

             return LiveVariables.SetEquals(other.LiveVariables);
        }
        
        public override int GetHashCode()
        {
            return LiveVariables.GetHashCode(); //Only used in a non readonly fashion in tests
        }
        
        public override string ToString()
        {
            var liveVariablesAsString = string.Join(", ", LiveVariables);

            return $"{{ {liveVariablesAsString} }}";
        }
        
    }
}