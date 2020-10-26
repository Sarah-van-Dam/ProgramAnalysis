using System;
using System.Collections.Generic;
using System.Linq;
using Analyses.Analysis.BitVector.LiveVariablesAnalysis;

namespace Analyses.Analysis.Distributive
{
    public class FaintVariableConstraint : IConstraints
    {
        public HashSet<string> StronglyLivedVariables { get; set; }

        public FaintVariableConstraint()
        {
            new HashSet<string>();
            StronglyLivedVariables = new HashSet<string>();
        }
        
        public bool IsSubset(IConstraints other)
        {
            if (!(other is FaintVariableConstraint otherFaintVariableConstraint))
            {
                return false;
            }
            
            
            //var faintVariablesIsSubset = FaintVariables.All(l => otherFaintVariableConstraint.FaintVariables.Contains(l));
            var stronglyLivedVariablesIsSubset = StronglyLivedVariables.All(l => otherFaintVariableConstraint.StronglyLivedVariables.Contains(l));
            //var noOverlap = !FaintVariables.Intersect(otherFaintVariableConstraint.StronglyLivedVariables)
            //    .Union((StronglyLivedVariables.Intersect(otherFaintVariableConstraint.FaintVariables))).Any();

            return  stronglyLivedVariablesIsSubset; // & faintVariablesIsSubset & noOverlap;
        }

        
        // It is a over-approximation, so we do not need the super set method.
        public bool IsSuperSet(IConstraints other)
        {
            throw new System.NotImplementedException();
        }

        public IConstraints Clone()
        {
            var clonedConstraints = new FaintVariableConstraint();
            foreach (var variable in StronglyLivedVariables)
            {
                clonedConstraints.StronglyLivedVariables.Add(variable);
            }

            return clonedConstraints;
        }

        public void Join(IConstraints other)
        {
            if (!(other is FaintVariableConstraint faintVariableConstraint))
            {
                throw new Exception($"Join operator called with constraint type that was different from {nameof(FaintVariableConstraint)}");
            }
            StronglyLivedVariables.UnionWith(faintVariableConstraint.StronglyLivedVariables);
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is FaintVariableConstraint other))
            {
                return false;
            }

            return StronglyLivedVariables.SetEquals(other.StronglyLivedVariables);
        }
        
        public override int GetHashCode()
        {
            return StronglyLivedVariables.GetHashCode(); //Only used in a non readonly fashion in tests
        }

    }
}