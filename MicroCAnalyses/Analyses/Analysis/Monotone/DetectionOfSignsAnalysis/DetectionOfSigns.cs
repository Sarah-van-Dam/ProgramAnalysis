using Analyses.Analysis.Actions;
using Analyses.Analysis.BitVector;
using Analyses.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Analyses.Analysis.Monotone.DetectionOfSignsAnalysis
{
	class DetectionOfSigns : BitVectorFramework
	{
		private readonly HashSet<(string variable, bool negative, bool zero, bool positive)> initialSigns;

		public DetectionOfSigns(ProgramGraph programGraph, HashSet<(string variable, bool negative, bool zero, bool positive)> initialSigns) : base(programGraph)
		{
			this.initialSigns = initialSigns;
		}

		public override void InitializeConstraints()
		{
			foreach (var node in _program.Nodes)
			{
				FinalConstraintsForNodes[node] = new DetectionOfSignsConstraint();
			}

			var startingConstraints = FinalConstraintsForNodes[_program.Nodes.Single(n => n.Name == ProgramGraph.StartNode)] as DetectionOfSignsConstraint;

			foreach ((string variable, bool negative, bool zero, bool positive) in initialSigns)
			{
                startingConstraints.VariableSigns[variable] = (variable, new Signs(negative, zero, positive));
			}
		}

        public override void Generate(Edge edge, IConstraints constraints)
        {
            if (!(constraints is DetectionOfSignsConstraint dosConstraints))
            {
                throw new InvalidOperationException($"Expected argument 'constraints' to be of type {nameof(DetectionOfSignsConstraint)}, but got {nameof(constraints)}.");
            }
            switch (edge.Action)
            {
                case IntDeclaration intDeclaration:
                    dosConstraints.VariableSigns[intDeclaration.VariableName] = (intDeclaration.VariableName, new Signs(false, true, false));
                    break;
                case ArrayDeclaration arrayDeclaration:
                    dosConstraints.VariableSigns[arrayDeclaration.ArrayName] = (arrayDeclaration.ArrayName, new Signs(false, true, false));
                    break;
                case Condition condition:
                    //A condition cannot generate anything
                    break;
                case RecordDeclaration recordDeclaration:
                    dosConstraints.VariableSigns[recordDeclaration.VariableName] = (recordDeclaration.VariableName, new Signs(false, true, false));
                    break;
                case IntAssignment intAssignment:
                    dosConstraints.VariableSigns[intAssignment.VariableName] = (intAssignment.VariableName, getSigns(dosConstraints, intAssignment.RightHandSide));
                    break;
                case ArrayAssignment arrayAssignment:
                    dosConstraints.VariableSigns[arrayAssignment.ArrayName] =
                        (arrayAssignment.ArrayName, dosConstraints.VariableSigns[arrayAssignment.ArrayName].signs.Union(getSigns(dosConstraints, arrayAssignment.RightHandSide)));
                    break;
                case RecordMemberAssignment recordMemberAssignment:
                    dosConstraints.VariableSigns[recordMemberAssignment.RecordName] =
                        (
                            recordMemberAssignment.RecordName,
                            dosConstraints.VariableSigns[recordMemberAssignment.RecordName].signs.Union(getSigns(dosConstraints, recordMemberAssignment.RightHandSide))
                        );
                    break;
                case RecordAssignment recordAssignment:
                    dosConstraints.VariableSigns[recordAssignment.RecordName] = (
                        recordAssignment.RecordName,
                        getSigns(dosConstraints, recordAssignment.FirstExpression).Union(getSigns(dosConstraints, recordAssignment.SecondExpression))
                    );
                    break;
                case ReadVariable read:
                    dosConstraints.VariableSigns[read.VariableName] = (read.VariableName, new Signs(true, true, true));
                    break;
                case ReadArray readArray:
                    dosConstraints.VariableSigns[readArray.ArrayName] = (readArray.ArrayName, new Signs(true, true, true));
                    break;
                case ReadRecordMember recordMember:
                    dosConstraints.VariableSigns[recordMember.RecordName] = (recordMember.RecordName, new Signs(true, true, true));
                    break;
                case Write write:
                    //A write cannot generate anything
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edge.Action), edge.Action,
                        $"No gen set has been generated for this action: {edge.Action} ");
            }
        }

        public override void Kill(Edge edge, IConstraints constraints)
        {
            if (!(constraints is DetectionOfSignsConstraint dosConstraints))
            {
                throw new InvalidOperationException($"Expected argument 'constraints' to be of type {nameof(DetectionOfSignsConstraint)}, but got {nameof(constraints)}.");
            }
            // Not entirely clear on this yet.
        }

        private Signs getSigns(DetectionOfSignsConstraint dosConstraints, MicroCTypes.arithmeticExpression expr)
		{
			switch (expr)
			{
				case MicroCTypes.arithmeticExpression.Number n:
					return new Signs(n.Item < 0, n.Item == 0, n.Item > 0);
				case MicroCTypes.arithmeticExpression.Variable var:
					return dosConstraints.VariableSigns[var.Item].signs;
				case MicroCTypes.arithmeticExpression.ArrayMember arrayMember:
					return dosConstraints.VariableSigns[arrayMember.Item1].signs;
				case MicroCTypes.arithmeticExpression.RecordMember recordMember:
					return dosConstraints.VariableSigns[recordMember.Item1].signs;
				case MicroCTypes.arithmeticExpression.Plus plus:
					var signsPlus = new Signs();
					var newSignsPlus = getSigns(dosConstraints, plus.Item1).ArithmeticPlus(getSigns(dosConstraints, plus.Item2));
					foreach (var newSign in newSignsPlus) signsPlus = signsPlus.Union(newSign);
					return signsPlus;
				case MicroCTypes.arithmeticExpression.Minus minus:
					var signsMinus = new Signs();
					var newSignsMinus = getSigns(dosConstraints, minus.Item1).ArithmeticPlus(getSigns(dosConstraints, minus.Item2));
					foreach (var newSign in newSignsMinus) signsMinus = signsMinus.Union(newSign);
					return signsMinus;
				case MicroCTypes.arithmeticExpression.Modulo modulo:
					return new Signs(false, true, getSigns(dosConstraints, modulo.Item2).Positive);
				case MicroCTypes.arithmeticExpression.Multiply multiply:
					var signsMultiply = new Signs();
					var newSignsMultiply = getSigns(dosConstraints, multiply.Item1).ArithmeticMultiply(getSigns(dosConstraints, multiply.Item2));
					foreach (var newSign in newSignsMultiply) signsMultiply = signsMultiply.Union(newSign);
					return signsMultiply;
				case MicroCTypes.arithmeticExpression.Power power:
					var signsPower = getSigns(dosConstraints, power.Item1);
					if (power.Item2 is MicroCTypes.arithmeticExpression.Number)
					{
						int n = (power.Item2 as MicroCTypes.arithmeticExpression.Number).Item;
						if (n < 0 && signsPower.Zero) signsPower = signsPower.Union(new Signs(false, false, true)); // In case of ex. 0 ^ -1.
						if (n % 2 == 0) signsPower = signsPower.Union(new Signs(false, false, true));
						if (n == 0) signsPower = signsPower.Union(new Signs(false, false, true));
					}
					else
					{
						signsPower = signsPower.Union(new Signs(false, false, true)); // Can't say much if all we know about the exponent is its sign.
					}
					return signsPower;
				default:
                    // Shouldn't be able to happen, unless we get more arithmeticExpression types.
                    throw new InvalidOperationException($"The arithmetic expression of type {nameof(expr)} is not yet supported for getSigns().");
			}
		}
	}
}
