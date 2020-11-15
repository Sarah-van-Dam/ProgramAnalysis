using System;
using System.Collections.Generic;
using System.Text;

namespace Analyses.Analysis.Distributive
{
    public class DangerousVariablesConstraint : IConstraints
    {
        public HashSet<string> DangerousVariables { get; set; }

        public DangerousVariablesConstraint()
        {
            new HashSet<string>();
            DangerousVariables = new HashSet<string>();
        }

        public IConstraints Clone()
        {
            var clonedConstraints = new DangerousVariablesConstraint();
            foreach (var variable in StronglyLivedVariables)
            {
                clonedConstraints.StronglyLivedVariables.Add(variable);
            }

            return clonedConstraints;
        }

        public bool IsSubset(IConstraints other)
        {
            throw new NotImplementedException();
        }

        public bool IsSuperSet(IConstraints other)
        {
            throw new NotImplementedException();
        }

        public void Join(IConstraints other)
        {
            throw new NotImplementedException();
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
