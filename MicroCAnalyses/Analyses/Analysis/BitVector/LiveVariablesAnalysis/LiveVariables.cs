using System;
using System.Collections.Generic;
using Analyses.Analysis.Actions;
using Analyses.Graph;

namespace Analyses.Analysis.BitVector.LiveVariablesAnalysis
{
    public class LiveVariables : BitVectorFramework
    {
        private readonly FreeVariablesAnalysis _freeVariablesAnalysis;

        public LiveVariables(ProgramGraph programGraph) : base(programGraph)
        {
            Direction = Direction.Backwards;
            JoinOperator = Operator.Union;
            _freeVariablesAnalysis = new FreeVariablesAnalysis();
        }

        public override void Kill(Edge edge, IConstraints constraints)
        {
            if (!(constraints is LiveVariableConstraint liveConstraints))
            {
                throw new Exception($"Something went wrong. It should only be possible to call with {nameof(LiveVariableConstraint)}");
            }

            switch (edge.Action)
            {
                case ArrayAssignment arrayAssignment:
                    // An array assignment cannot kill anything
                    break;
                case ArrayDeclaration arrayDeclaration:
                    liveConstraints.LiveVariables.Remove(arrayDeclaration.ArrayName);
                    break;
                case Condition condition:
                    // A condition cannot kill anything
                    break;
                case IntAssignment intAssignment:
                    liveConstraints.LiveVariables.Remove(intAssignment.VariableName);
                    break;
                case IntDeclaration intDeclaration:
                    liveConstraints.LiveVariables.Remove(intDeclaration.VariableName);
                    break;
                case ReadArray readArray:
                    // A read array cannot kill anything
                    break;
                case ReadRecordMember readRecordMember:
                    liveConstraints.LiveVariables.Remove($"{readRecordMember.RecordName}.{readRecordMember.RecordMember}");
                    break;
                case ReadVariable readVariable:
                    liveConstraints.LiveVariables.Remove(readVariable.VariableName);
                    break;
                case RecordAssignment recordAssignment:
                    liveConstraints.LiveVariables.Remove($"{recordAssignment.RecordName}.{RecordMember.Fst}");
                    liveConstraints.LiveVariables.Remove($"{recordAssignment.RecordName}.{RecordMember.Snd}");
                    break;
                case RecordDeclaration recordDeclaration:
                    liveConstraints.LiveVariables.Remove($"{recordDeclaration.VariableName}.{RecordMember.Fst}");
                    liveConstraints.LiveVariables.Remove($"{recordDeclaration.VariableName}.{RecordMember.Snd}");
                    break;
                case RecordMemberAssignment recordMemberAssignment:
                    liveConstraints.LiveVariables.Remove($"{recordMemberAssignment.RecordName}.{recordMemberAssignment.RecordMember}");
                    break;
                case Write write:
                    // A write cannot kill anything
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Generate(Edge edge, IConstraints constraints)
        {
            if (!(constraints is LiveVariableConstraint liveConstraints))
            {
                throw new Exception($"Something went wrong. It should only be possible to call with {nameof(LiveVariableConstraint)}");
            }

            switch (edge.Action)
            {
                case ArrayAssignment arrayAssignment:
                    liveConstraints.LiveVariables.UnionWith(_freeVariablesAnalysis.FreeVariables(arrayAssignment.Index));
                    liveConstraints.LiveVariables.UnionWith(_freeVariablesAnalysis.FreeVariables(arrayAssignment.RightHandSide));
                    break;
                case ArrayDeclaration arrayDeclaration:
                    // An array declaration cannot generate anything
                    break;
                case Condition condition:
                    liveConstraints.LiveVariables.UnionWith(_freeVariablesAnalysis.FreeVariables(condition.Cond));
                    break;
                case IntAssignment intAssignment:
                    liveConstraints.LiveVariables.UnionWith(_freeVariablesAnalysis.FreeVariables(intAssignment.RightHandSide));
                    break;
                case IntDeclaration intDeclaration:
                    // An int declaration cannot generate anything
                    break;
                case ReadArray readArray:
                    liveConstraints.LiveVariables.UnionWith(_freeVariablesAnalysis.FreeVariables(readArray.Index));
                    break;
                case ReadRecordMember readRecordMember:
                    // A record member cannot generate anything
                    break;
                case ReadVariable readVariable:
                    // A variable cannot generate anything
                    break;
                case RecordAssignment recordAssignment:
                    liveConstraints.LiveVariables.UnionWith(_freeVariablesAnalysis.FreeVariables(recordAssignment.FirstExpression));
                    liveConstraints.LiveVariables.UnionWith(_freeVariablesAnalysis.FreeVariables(recordAssignment.SecondExpression));
                    break;
                case RecordDeclaration recordDeclaration:
                    // A record declaration cannot generate anything
                    break;
                case RecordMemberAssignment recordMemberAssignment:
                    liveConstraints.LiveVariables.UnionWith(_freeVariablesAnalysis.FreeVariables(recordMemberAssignment.RightHandSide));
                    break;
                case Write write:
                    liveConstraints.LiveVariables.UnionWith(_freeVariablesAnalysis.FreeVariables(write.Expression));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void InitializeConstraints()
        {
            foreach (var node in _program.Nodes)
            {
                FinalConstraintsForNodes[node] = new LiveVariableConstraint();
            }
        }
    }
}