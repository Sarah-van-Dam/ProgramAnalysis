using System;
using System.Collections.Generic;
using Analyses.Analysis.Actions;
using Analyses.Graph;

namespace Analyses.Analysis.BitVector.LiveVariablesAnalysis
{
    public class LiveVariables : BitVectorFramework
    {
        public LiveVariables(ProgramGraph programGraph) : base(programGraph)
        {
            Direction = Direction.Backwards;
            JoinOperator = Operator.Union;
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
                    liveConstraints.LiveVariables.UnionWith(FreeVariables(arrayAssignment.Index));
                    liveConstraints.LiveVariables.UnionWith(FreeVariables(arrayAssignment.RightHandSide));
                    break;
                case ArrayDeclaration arrayDeclaration:
                    // An array declaration cannot generate anything
                    break;
                case Condition condition:
                    liveConstraints.LiveVariables.UnionWith(FreeVariables(condition.Cond));
                    break;
                case IntAssignment intAssignment:
                    liveConstraints.LiveVariables.UnionWith(FreeVariables(intAssignment.RightHandSide));
                    break;
                case IntDeclaration intDeclaration:
                    // An int declaration cannot generate anything
                    break;
                case ReadArray readArray:
                    liveConstraints.LiveVariables.UnionWith(FreeVariables(readArray.Index));
                    break;
                case ReadRecordMember readRecordMember:
                    // A record member cannot generate anything
                    break;
                case ReadVariable readVariable:
                    // A variable cannot generate anything
                    break;
                case RecordAssignment recordAssignment:
                    liveConstraints.LiveVariables.UnionWith(FreeVariables(recordAssignment.FirstExpression));
                    liveConstraints.LiveVariables.UnionWith(FreeVariables(recordAssignment.SecondExpression));
                    break;
                case RecordDeclaration recordDeclaration:
                    // A record declaration cannot generate anything
                    break;
                case RecordMemberAssignment recordMemberAssignment:
                    liveConstraints.LiveVariables.UnionWith(FreeVariables(recordMemberAssignment.RightHandSide));
                    break;
                case Write write:
                    liveConstraints.LiveVariables.UnionWith(FreeVariables(write.Expression));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HashSet<string> FreeVariables(MicroCTypes.booleanExpression booleanExpression)
        {
            var freeVariables = new HashSet<string>();
            switch (booleanExpression)
            {
                case MicroCTypes.booleanExpression.And @and:
                    freeVariables.UnionWith(FreeVariables(@and.Item1));
                    freeVariables.UnionWith(FreeVariables(@and.Item2));
                    break;
                case MicroCTypes.booleanExpression.Equal equal:
                    freeVariables.UnionWith(FreeVariables(equal.Item1));
                    freeVariables.UnionWith(FreeVariables(equal.Item2));
                    break;
                case MicroCTypes.booleanExpression.Great great:
                    freeVariables.UnionWith(FreeVariables(great.Item1));
                    freeVariables.UnionWith(FreeVariables(great.Item2));
                    break;
                case MicroCTypes.booleanExpression.GreatEqual greatEqual:
                    freeVariables.UnionWith(FreeVariables(greatEqual.Item1));
                    freeVariables.UnionWith(FreeVariables(greatEqual.Item2));
                    break;
                case MicroCTypes.booleanExpression.Less less:
                    freeVariables.UnionWith(FreeVariables(less.Item1));
                    freeVariables.UnionWith(FreeVariables(less.Item2));
                    break;
                case MicroCTypes.booleanExpression.LessEqual lessEqual:
                    freeVariables.UnionWith(FreeVariables(lessEqual.Item1));
                    freeVariables.UnionWith(FreeVariables(lessEqual.Item2));
                    break;
                case MicroCTypes.booleanExpression.Not @not:
                    freeVariables.UnionWith(FreeVariables(@not.Item));
                    break;
                case MicroCTypes.booleanExpression.NotEqual notEqual:
                    freeVariables.UnionWith(FreeVariables(notEqual.Item1));
                    freeVariables.UnionWith(FreeVariables(notEqual.Item2));
                    break;
                case MicroCTypes.booleanExpression.Or @or:
                    freeVariables.UnionWith(FreeVariables(@or.Item1));
                    freeVariables.UnionWith(FreeVariables(@or.Item2));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(booleanExpression));
            }

            return freeVariables;
        }

        private HashSet<string> FreeVariables(MicroCTypes.arithmeticExpression arithmeticExpression)
        {
            var freeVariables = new HashSet<string>();
            switch (arithmeticExpression)
            {
                case MicroCTypes.arithmeticExpression.ArrayMember arrayMember:
                    freeVariables.Add(arrayMember.Item1);
                    freeVariables.UnionWith(FreeVariables(arrayMember.Item2));
                    break;
                case MicroCTypes.arithmeticExpression.Divide divide:
                    freeVariables.UnionWith(FreeVariables(divide.Item1));
                    freeVariables.UnionWith(FreeVariables(divide.Item2));
                    break;
                case MicroCTypes.arithmeticExpression.Minus minus:
                    freeVariables.UnionWith(FreeVariables(minus.Item1));
                    freeVariables.UnionWith(FreeVariables(minus.Item2));
                    break;
                case MicroCTypes.arithmeticExpression.Modulo modulo:
                    freeVariables.UnionWith(FreeVariables(modulo.Item1));
                    freeVariables.UnionWith(FreeVariables(modulo.Item2));
                    break;
                case MicroCTypes.arithmeticExpression.Multiply multiply:
                    freeVariables.UnionWith(FreeVariables(multiply.Item1));
                    freeVariables.UnionWith(FreeVariables(multiply.Item2));
                    break;
                case MicroCTypes.arithmeticExpression.Number number:
                    // A number isn't a free variable
                    break;
                case MicroCTypes.arithmeticExpression.Plus plus:
                    freeVariables.UnionWith(FreeVariables(plus.Item1));
                    freeVariables.UnionWith(FreeVariables(plus.Item2));
                    break;
                case MicroCTypes.arithmeticExpression.Power power:
                    freeVariables.UnionWith(FreeVariables(power.Item1));
                    freeVariables.UnionWith(FreeVariables(power.Item2));
                    break;
                case MicroCTypes.arithmeticExpression.RecordMember recordMember:
                    freeVariables.Add(recordMember.Item2 == 1 ? 
                          $"{recordMember.Item1}.{RecordMember.Fst}"
                        : $"{recordMember.Item1}.{RecordMember.Snd}");
                    break;
                case MicroCTypes.arithmeticExpression.Variable variable:
                    freeVariables.Add(variable.Item);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(arithmeticExpression));
            }

            return freeVariables;
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