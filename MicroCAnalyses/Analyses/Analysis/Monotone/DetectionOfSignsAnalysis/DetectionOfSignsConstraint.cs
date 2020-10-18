using System;
using System.Collections.Generic;
using System.Linq;

namespace Analyses.Analysis.Monotone.DetectionOfSignsAnalysis
{
	class DetectionOfSignsConstraint : IConstraints
	{
		// The hashset used here contains information about where in the graph we encounter the variable, and what possible signs the value may have at that point (-, 0, +).
		// variable -> (variable, negative, zero, positive)
		public Dictionary<string, (string variable, Signs signs)> VariableSigns;

		public DetectionOfSignsConstraint()
		{
			VariableSigns = new Dictionary<string, (string variable, Signs signs)>();
		}

		public IConstraints Clone()
		{
			var clonedConstraints = new DetectionOfSignsConstraint();
			foreach (var variableSigns in VariableSigns)
			{
				clonedConstraints.VariableSigns[variableSigns.Key] = variableSigns.Value;
			}

			return clonedConstraints;
		}

		public bool IsSubset(IConstraints other)
		{
			if (!(other is DetectionOfSignsConstraint otherDetectionOfSigns))
			{
				return false;
			}

			foreach (var entry in VariableSigns)
			{
				var containsKey =
					otherDetectionOfSigns.VariableSigns.TryGetValue(entry.Key,
						out var otherConstraintNodeSet);
				if (!containsKey)
				{
					return false;
				}

				if (otherConstraintNodeSet.variable != entry.Value.variable || otherConstraintNodeSet.signs != entry.Value.signs)
				{
					return false;
				}
			}

			return true;
		}

		public bool IsSuperSet(IConstraints other)
		{
			throw new NotImplementedException();
		}

		public void Join(IConstraints other)
		{
			if (!(other is DetectionOfSignsConstraint detectionOfSignsConstraints))
			{
				throw new Exception($"Join operator called with constraint type that was different from {nameof(DetectionOfSignsConstraint)}");
			}
			foreach (var (key, set) in detectionOfSignsConstraints.VariableSigns)
			{
				var keyExists = VariableSigns.TryGetValue(key, out var existingSet);
				if (!keyExists)
				{
					existingSet = (key, new Signs());
					VariableSigns[key] = existingSet;
				}
				existingSet.signs = existingSet.signs.Union(set.signs);
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is DetectionOfSignsConstraint other))
			{
				return false;
			}

			return VariableSigns.Keys.SequenceEqual(other.VariableSigns.Keys)
				   && VariableSigns.All(kvp => kvp.Value == other.VariableSigns[kvp.Key]);
		}

		public override int GetHashCode()
		{
			return VariableSigns.GetHashCode();
		}
	}
}
