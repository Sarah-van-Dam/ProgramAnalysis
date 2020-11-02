using Analyses.Analysis.Actions;
using Analyses.Analysis.BitVector;
using Analyses.Analysis.Distributive;
using Analyses.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Analyses.Analysis.Monotone.DetectionOfSignsAnalysis
{
	class DetectionOfSigns : DistributiveFramework
	{
        public DetectionOfSigns(ProgramGraph graph) : base(graph) { }

		public override void InitializeConstraints()
		{
			foreach (var node in _program.Nodes)
			{
				FinalConstraintsForNodes[node] = new DetectionOfSignsConstraint();
			}

			var startingConstraints = FinalConstraintsForNodes[_program.Nodes.Single(n => n.Name == ProgramGraph.StartNode)] as DetectionOfSignsConstraint;

            foreach (string variable in _program.VariableNames)
            {
                startingConstraints.VariableSigns[variable] = (variable, new HashSet<Sign>());
            }
		}

        protected override void AnalysisFunction(Edge edge, IConstraints constraints)
        {
            if (!(constraints is DetectionOfSignsConstraint dosConstraints))
            {
                throw new InvalidOperationException($"Expected argument 'constraints' to be of type {nameof(DetectionOfSignsConstraint)}, but got {nameof(constraints)}.");
            }
            switch (edge.Action)
            {
                case IntDeclaration intDeclaration:
                    dosConstraints.VariableSigns[intDeclaration.VariableName] = (intDeclaration.VariableName, new HashSet<Sign>() { Sign.Zero });
                    break;
                case ArrayDeclaration arrayDeclaration:
                    dosConstraints.VariableSigns[arrayDeclaration.ArrayName] = (arrayDeclaration.ArrayName, new HashSet<Sign>() { Sign.Zero });
                    break;
                case Condition condition:
                    //A condition does not affect accessors
                    break;
                case RecordDeclaration recordDeclaration:
                    dosConstraints.VariableSigns[$"{recordDeclaration.VariableName}.{RecordMember.Fst}"] =
                        ($"{recordDeclaration.VariableName}.{RecordMember.Fst}", new HashSet<Sign>() { Sign.Zero });
                    dosConstraints.VariableSigns[$"{recordDeclaration.VariableName}.{RecordMember.Snd}"] =
                        ($"{recordDeclaration.VariableName}.{RecordMember.Snd}", new HashSet<Sign>() { Sign.Zero });
                    break;
                case IntAssignment intAssignment:
                    dosConstraints.VariableSigns[intAssignment.VariableName] = (intAssignment.VariableName, dosConstraints.getSigns(intAssignment.RightHandSide));
                    break;
                case ArrayAssignment arrayAssignment:
                    dosConstraints.VariableSigns[arrayAssignment.ArrayName].signs.UnionWith(dosConstraints.getSigns(arrayAssignment.RightHandSide));
                    break;
                case RecordMemberAssignment recordMemberAssignment:
                    dosConstraints.VariableSigns[$"{recordMemberAssignment.RecordName}.{recordMemberAssignment.RecordMember}"] =
                        ($"{recordMemberAssignment.RecordName}.{recordMemberAssignment.RecordMember}", dosConstraints.getSigns(recordMemberAssignment.RightHandSide));
                    break;
                case RecordAssignment recordAssignment:
                    dosConstraints.VariableSigns[$"{recordAssignment.RecordName}.{RecordMember.Fst}"] =
                        ($"{recordAssignment.RecordName}.{RecordMember.Fst}", dosConstraints.getSigns(recordAssignment.FirstExpression));
                    dosConstraints.VariableSigns[$"{recordAssignment.RecordName}.{RecordMember.Snd}"] =
                        ($"{recordAssignment.RecordName}.{RecordMember.Snd}", dosConstraints.getSigns(recordAssignment.SecondExpression));
                    break;
                case ReadVariable read:
                    dosConstraints.VariableSigns[read.VariableName].signs.Add(Sign.Negative);
                    dosConstraints.VariableSigns[read.VariableName].signs.Add(Sign.Zero);
                    dosConstraints.VariableSigns[read.VariableName].signs.Add(Sign.Positive);
                    break;
                case ReadArray readArray:
                    dosConstraints.VariableSigns[readArray.ArrayName].signs.Add(Sign.Negative);
                    dosConstraints.VariableSigns[readArray.ArrayName].signs.Add(Sign.Zero);
                    dosConstraints.VariableSigns[readArray.ArrayName].signs.Add(Sign.Positive);
                    break;
                case ReadRecordMember recordMember:
                    dosConstraints.VariableSigns[$"{recordMember.RecordName}.{recordMember.RecordMember}"].signs.Add(Sign.Negative);
                    dosConstraints.VariableSigns[$"{recordMember.RecordName}.{recordMember.RecordMember}"].signs.Add(Sign.Zero);
                    dosConstraints.VariableSigns[$"{recordMember.RecordName}.{recordMember.RecordMember}"].signs.Add(Sign.Positive);
                    break;
                case Write write:
                    //A write cannot generate anything
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edge.Action), edge.Action,
                        $"No gen set has been generated for this action: {edge.Action} ");
            }
        }

	}
}
