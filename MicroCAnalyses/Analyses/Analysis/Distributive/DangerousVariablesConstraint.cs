using System;
using System.Collections.Generic;
using System.Linq;
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
            foreach (var variable in DangerousVariables)
            {
                clonedConstraints.DangerousVariables.Add(variable);
            }

            return clonedConstraints;
        }

        public bool IsSubset(IConstraints other)
        {
            if (!(other is DangerousVariablesConstraint otherDangerousVariableConstraint))
            {
                return false;
            }
            var dangerousVariablesIsSubset = DangerousVariables.All(l => otherDangerousVariableConstraint.DangerousVariables.Contains(l));
            return dangerousVariablesIsSubset;
        }

        public bool IsSuperSet(IConstraints other)
        {
            throw new NotImplementedException();
        }

        public void Join(IConstraints other)
        {
            if (!(other is DangerousVariablesConstraint dangerousVariableConstraint))
            {
                throw new Exception($"Join operator called with constraint type that was different from {nameof(DangerousVariablesConstraint)}");
            }
            DangerousVariables.UnionWith(dangerousVariableConstraint.DangerousVariables);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is DangerousVariablesConstraint other))
            {
                return false;
            }

            return DangerousVariables.SetEquals(other.DangerousVariables);
        }

        public override int GetHashCode()
        {
            return DangerousVariables.GetHashCode(); //Only used in a non readonly fashion in tests
        }
    }
}
