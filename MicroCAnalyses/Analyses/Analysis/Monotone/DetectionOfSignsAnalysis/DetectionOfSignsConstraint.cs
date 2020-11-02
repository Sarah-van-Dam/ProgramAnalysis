using Analyses.Analysis.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Analyses.Analysis.Monotone.DetectionOfSignsAnalysis
{
	class DetectionOfSignsConstraint : IConstraints
	{
		// The hashset used here contains information about where in the graph we encounter the variable, and what possible signs the value may have at that point (-, 0, +).
		// variable -> (variable, negative, zero, positive)
		public Dictionary<string, (string variable, HashSet<Sign> signs)> VariableSigns;

		public DetectionOfSignsConstraint()
		{
			VariableSigns = new Dictionary<string, (string variable, HashSet<Sign> signs)>();
		}

		public override string ToString()
			=> "Detection of Signs constraint\n" + string.Join("\n", VariableSigns.Select(entry => $"({entry.Key}, {{{string.Join(", ", entry.Value.signs)}}})"));

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

				if (!otherConstraintNodeSet.signs.IsSupersetOf(entry.Value.signs))
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
					existingSet = (key, new HashSet<Sign>());
					VariableSigns[key] = existingSet;
				}
				existingSet.signs.UnionWith(set.signs);
			}
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is DetectionOfSignsConstraint other))
			{
				return false;
			}

			return VariableSigns.Keys.SequenceEqual(other.VariableSigns.Keys)
				   && VariableSigns.All(kvp =>
					   kvp.Value.signs.SetEquals(other.VariableSigns[kvp.Key].signs));
		}

		public override int GetHashCode()
		{
			return VariableSigns.GetHashCode();
		}


		public HashSet<Sign> getSigns(MicroCTypes.arithmeticExpression expr)
		{
			switch (expr)
			{
				case MicroCTypes.arithmeticExpression.Number n:
					if (n.Item < 0)
						return new HashSet<Sign>() { Sign.Negative };
					else if (n.Item == 0)
						return new HashSet<Sign>() { Sign.Zero };
					else
						return new HashSet<Sign>() { Sign.Positive };
				case MicroCTypes.arithmeticExpression.Variable var:
					return VariableSigns[var.Item].signs;
				case MicroCTypes.arithmeticExpression.ArrayMember arrayMember:
					return VariableSigns[arrayMember.Item1].signs;
				case MicroCTypes.arithmeticExpression.RecordMember recordMember:
					return VariableSigns[$"{recordMember.Item1}.{(recordMember.Item2 == 1 ? RecordMember.Fst : RecordMember.Snd)}"].signs;
				case MicroCTypes.arithmeticExpression.Plus plus:
					return ArithmeticOperation(getSigns(plus.Item1), getSigns(plus.Item2), OperandTablePlus);
				case MicroCTypes.arithmeticExpression.Minus minus:
					return ArithmeticOperation(getSigns(minus.Item1), getSigns(minus.Item2), OperandTableMinus);
				case MicroCTypes.arithmeticExpression.Modulo modulo:
					var signsModulo = new HashSet<Sign>() { Sign.Zero };
					if (getSigns(modulo.Item2).Contains(Sign.Positive))
						signsModulo.Add(Sign.Positive);
					return signsModulo;
				case MicroCTypes.arithmeticExpression.Multiply multiply:
					return ArithmeticOperation(getSigns(multiply.Item1), getSigns(multiply.Item2), OperandTableMultiply);
				case MicroCTypes.arithmeticExpression.Power power:
					var signsPower = getSigns(power.Item1);
					if (power.Item2 is MicroCTypes.arithmeticExpression.Number)
					{
						int n = (power.Item2 as MicroCTypes.arithmeticExpression.Number).Item;
						if (n < 0 && signsPower.Contains(Sign.Zero)) signsPower.Add(Sign.Positive); // In case of ex. -n ^ 0 resulting in 1.
						if (n % 2 == 0) signsPower.Add(Sign.Positive); // n ^ 2, n ^ 4, n ^ 128, ... all give positive results.
						if (n == 0) signsPower.Add(Sign.Positive);
					}
					else
					{
						signsPower.Add(Sign.Positive); // Can't say much if all we know about the exponent is its sign. Can possibly be positive now.
					}
					return signsPower;
				default:
					// Shouldn't be able to happen, unless we get more arithmeticExpression types.
					throw new InvalidOperationException($"The arithmetic expression of type {nameof(expr)} is not yet supported for getSigns().");
			}
		}

		// 3x3 matrix of how different signs behave with the + operator. In form of (leftSide, rightSide, resultingSigns).
		private static HashSet<(Sign, Sign, IEnumerable<Sign>)> OperandTablePlus = new HashSet<(Sign, Sign, IEnumerable<Sign>)>()
		{
			( Sign.Negative, Sign.Negative, new[] { Sign.Negative } ),
			( Sign.Zero, Sign.Negative, new[] { Sign.Negative } ),
			( Sign.Positive, Sign.Negative, new[] { Sign.Negative, Sign.Zero, Sign.Positive } ),
			( Sign.Negative, Sign.Zero, new[] { Sign.Negative } ),
			( Sign.Zero, Sign.Zero, new[] { Sign.Zero } ),
			( Sign.Positive, Sign.Zero, new[] { Sign.Positive } ),
			( Sign.Negative, Sign.Positive, new[] { Sign.Negative, Sign.Zero, Sign.Positive } ),
			( Sign.Zero, Sign.Positive, new[] { Sign.Positive } ),
			( Sign.Positive, Sign.Positive, new[] { Sign.Positive } )
		};
		// 3x3 matrix of how different signs behave with the - operator. In form of (leftSide, rightSide, resultingSigns).
		private static HashSet<(Sign, Sign, IEnumerable<Sign>)> OperandTableMinus = new HashSet<(Sign, Sign, IEnumerable<Sign>)>()
		{
			( Sign.Negative, Sign.Negative, new[] { Sign.Negative, Sign.Zero, Sign.Positive } ),
			( Sign.Zero, Sign.Negative, new[] { Sign.Positive } ),
			( Sign.Positive, Sign.Negative, new[] { Sign.Positive } ),
			( Sign.Negative, Sign.Zero, new[] { Sign.Negative } ),
			( Sign.Zero, Sign.Zero, new[] { Sign.Zero } ),
			( Sign.Positive, Sign.Zero, new[] { Sign.Positive } ),
			( Sign.Negative, Sign.Positive, new[] { Sign.Negative } ),
			( Sign.Zero, Sign.Positive, new[] { Sign.Negative } ),
			( Sign.Positive, Sign.Positive, new[] { Sign.Negative, Sign.Zero, Sign.Positive } )
		};
		// 3x3 matrix of how different signs behave with the * operator. In form of (leftSide, rightSide, resultingSigns).
		private static HashSet<(Sign, Sign, IEnumerable<Sign>)> OperandTableMultiply = new HashSet<(Sign, Sign, IEnumerable<Sign>)>()
		{
			( Sign.Negative, Sign.Negative, new[] { Sign.Positive } ),
			( Sign.Zero, Sign.Negative, new[] { Sign.Zero } ),
			( Sign.Positive, Sign.Negative, new[] { Sign.Negative } ),
			( Sign.Negative, Sign.Zero, new[] { Sign.Zero } ),
			( Sign.Zero, Sign.Zero, new[] { Sign.Zero } ),
			( Sign.Positive, Sign.Zero, new[] { Sign.Zero } ),
			( Sign.Negative, Sign.Positive, new[] { Sign.Negative } ),
			( Sign.Zero, Sign.Positive, new[] { Sign.Zero } ),
			( Sign.Positive, Sign.Positive, new[] { Sign.Positive } )
		};
		
		public HashSet<Sign> ArithmeticOperation(HashSet<Sign> leftSide, HashSet<Sign> rightSide, HashSet<(Sign, Sign, IEnumerable<Sign>)> operandTable)
		{
			HashSet<Sign> result = new HashSet<Sign>();
			foreach ((Sign leftCheck, Sign rightCheck, IEnumerable<Sign> resultSigns) in operandTable)
			{
				if (leftSide.Contains(leftCheck) && rightSide.Contains(rightCheck))
				{
					result.UnionWith(resultSigns);
				}
			}
			return result;
		}

	}
}
