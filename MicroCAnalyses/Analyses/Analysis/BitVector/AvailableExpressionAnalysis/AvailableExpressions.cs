using System;
using System.Collections.Generic;
using System.Linq;
using Analyses.Algorithms;
using Analyses.Analysis.Actions;
using Analyses.Analysis.BitVector.LiveVariablesAnalysis;
using Analyses.Graph;

namespace Analyses.Analysis.BitVector.AvailableExpressionAnalysis
{
    public class AvailableExpressions : BitVectorFramework
    {
        private FreeVariablesAnalysis _freeVariables;

        public AvailableExpressions(ProgramGraph programGraph, WorklistImplementation worklistImplementation)
            : base(programGraph, Direction.Forward, worklistImplementation)
        {
            JoinOperator = Operator.Intersection;
            _freeVariables = new FreeVariablesAnalysis();
        }

        public override void Kill(Edge edge, IConstraints constraints)
        {
            if (!(constraints is AvailableExpressionConstraints availableExpressionConstraints))
            {
                throw new Exception(
                    $"Something went wrong. It should only be possible to call with {nameof(AvailableExpressionConstraints)}");
            }

            switch (edge.Action)
            {
                case ArrayAssignment arrayAssignment:
                    KillAssignment(availableExpressionConstraints, arrayAssignment.ArrayName);
                    break;
                case ArrayDeclaration arrayDeclaration:
                    break;
                case Condition condition:
                    break;
                case IntAssignment intAssignment:
                    KillAssignment(availableExpressionConstraints, intAssignment.VariableName);
                    break;
                case IntDeclaration intDeclaration:
                    break;
                case ReadArray readArray:
                    KillAssignment(availableExpressionConstraints, readArray.ArrayName);
                    break;
                case ReadRecordMember readRecordMember:
                    KillAssignment(availableExpressionConstraints,
                        $"{readRecordMember.RecordName}.{readRecordMember.RecordMember}");
                    break;
                case ReadVariable readVariable:
                    KillAssignment(availableExpressionConstraints, readVariable.VariableName);
                    break;
                case RecordAssignment recordAssignment:
                    KillAssignment(availableExpressionConstraints, $"{recordAssignment.RecordName}.{RecordMember.Fst}");
                    KillAssignment(availableExpressionConstraints, $"{recordAssignment.RecordName}.{RecordMember.Snd}");
                    break;
                case RecordDeclaration recordDeclaration:
                    break;
                case RecordMemberAssignment recordMemberAssignment:
                    KillAssignment(availableExpressionConstraints,
                        $"{recordMemberAssignment.RecordName}.{recordMemberAssignment.RecordMember}");
                    break;
                case Write write:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public override void Generate(Edge edge, IConstraints constraints)
        {
            if (!(constraints is AvailableExpressionConstraints availableExpressionConstraints))
            {
                throw new Exception(
                    $"Something went wrong. It should only be possible to call with {nameof(AvailableExpressionConstraints)}");
            }

            switch (edge.Action)
            {
                case ArrayAssignment arrayAssignment:
                    var expressionAvailableExpressions = AE(arrayAssignment.RightHandSide);
                    expressionAvailableExpressions.UnionWith(AE(arrayAssignment.Index));
                    GenerateAssignment(availableExpressionConstraints, expressionAvailableExpressions,
                        arrayAssignment.ArrayName);
                    break;
                case ArrayDeclaration arrayDeclaration:
                    break;
                case Condition condition:
                    var (availableArithmeticExpressions, availableBooleanExpressions) = AE(condition.Cond);
                    availableExpressionConstraints.AvailableArithmeticExpressions.UnionWith(
                        availableArithmeticExpressions);
                    availableExpressionConstraints.AvailableBooleanExpressions.UnionWith(availableBooleanExpressions);
                    break;
                case IntAssignment intAssignment:
                    expressionAvailableExpressions = AE(intAssignment.RightHandSide);
                    GenerateAssignment(availableExpressionConstraints, expressionAvailableExpressions,
                        intAssignment.VariableName);
                    break;
                case IntDeclaration intDeclaration:
                    break;
                case ReadArray readArray:
                    expressionAvailableExpressions = AE(readArray.Index);
                    GenerateAssignment(availableExpressionConstraints, expressionAvailableExpressions,
                        readArray.ArrayName);
                    break;
                case ReadRecordMember readRecordMember:
                    break;
                case ReadVariable readVariable:
                    break;
                case RecordAssignment recordAssignment:
                    var memberFirstAvailableExpressions = AE(recordAssignment.FirstExpression);
                    GenerateAssignment(availableExpressionConstraints, memberFirstAvailableExpressions,
                        $"{recordAssignment.RecordName}.{RecordMember.Fst}");

                    var memberSecondAvailableExpressions = AE(recordAssignment.SecondExpression);
                    GenerateAssignment(availableExpressionConstraints, memberFirstAvailableExpressions,
                        $"{recordAssignment.RecordName}.{RecordMember.Snd}");
                    break;
                case RecordDeclaration recordDeclaration:
                    break;
                case RecordMemberAssignment recordMemberAssignment:
                    expressionAvailableExpressions = AE(recordMemberAssignment.RightHandSide);
                    GenerateAssignment(availableExpressionConstraints, expressionAvailableExpressions,
                        $"{recordMemberAssignment.RecordName}.{recordMemberAssignment.RecordMember}");
                    break;
                case Write write:
                    expressionAvailableExpressions = AE(write.Expression);
                    availableExpressionConstraints.AvailableArithmeticExpressions.UnionWith(
                        expressionAvailableExpressions);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void InitializeConstraints()
        {
            //Find all subexpressions in program
            var (arithmeticSubExpressions, booleanSubExpressions) 
                = CalculateAllSubExpressions();
            foreach (var node in _program.Nodes)
            {
                //For each node that is not the first, set the constraints to be all subexpressions
                FinalConstraintsForNodes[node] = new AvailableExpressionConstraints(arithmeticSubExpressions, booleanSubExpressions);
            }
            
            //For first node set constraints to empty set
            throw new System.NotImplementedException();
        }

        private (HashSet<MicroCTypes.arithmeticExpression> arithmeticExpressions, 
            HashSet<MicroCTypes.booleanExpression> booleanExpressions
            ) CalculateAllSubExpressions()
        {
            var arithmeticExpressions = new HashSet<MicroCTypes.arithmeticExpression>();
            var booleanExpressions = new HashSet<MicroCTypes.booleanExpression>();
            foreach (var edge in _program.Edges)
            {
                switch (edge.Action)
                {
                    case ArrayAssignment arrayAssignment:
                        arithmeticExpressions.UnionWith(AE(arrayAssignment.Index));
                        arithmeticExpressions.UnionWith(AE(arrayAssignment.RightHandSide));
                        break;
                    case ArrayDeclaration arrayDeclaration:
                        break;
                    case Condition condition:
                        var (arithmetic, boolean) = AE(condition.Cond);
                        arithmeticExpressions.UnionWith(arithmetic);
                        booleanExpressions.UnionWith(boolean);
                        break;
                    case IntAssignment intAssignment:
                        arithmeticExpressions.UnionWith(AE(intAssignment.RightHandSide));
                        break;
                    case IntDeclaration intDeclaration:
                        break;
                    case ReadArray readArray:
                        arithmeticExpressions.UnionWith(AE(readArray.Index));
                        break;
                    case ReadRecordMember readRecordMember:
                        break;
                    case ReadVariable readVariable:
                        break;
                    case RecordAssignment recordAssignment:
                        arithmeticExpressions.UnionWith(AE(recordAssignment.FirstExpression));
                        arithmeticExpressions.UnionWith(AE(recordAssignment.SecondExpression));
                        break;
                    case RecordDeclaration recordDeclaration:
                        break;
                    case RecordMemberAssignment recordMemberAssignment:
                        arithmeticExpressions.UnionWith(AE(recordMemberAssignment.RightHandSide));
                        break;
                    case Write write:
                        arithmeticExpressions.UnionWith(AE(write.Expression));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return (arithmeticExpressions, booleanExpressions);
        }

        private (HashSet<MicroCTypes.arithmeticExpression> availableArithmeticExpressions,
            HashSet<MicroCTypes.booleanExpression> availableBooleanExpressions
            ) AE(MicroCTypes.booleanExpression booleanExpression)
        {
            var availableArithmeticExpressions = new HashSet<MicroCTypes.arithmeticExpression>();
            var availableBooleanExpressions = new HashSet<MicroCTypes.booleanExpression>();

            switch (booleanExpression)
            {
                case MicroCTypes.booleanExpression.And @and:
                    var (newAvailableArithmetic1, newAvailableBoolean1) = AE(@and.Item1);
                    var (newAvailableArithmetic2, newAvailableBoolean2) = AE(@and.Item2);
                    availableBooleanExpressions.Add(@and);
                    availableBooleanExpressions.UnionWith(newAvailableBoolean1.Union(newAvailableBoolean2));
                    availableArithmeticExpressions.UnionWith(newAvailableArithmetic1.Union(newAvailableArithmetic2));
                    break;
                case MicroCTypes.booleanExpression.Equal equal:
                    newAvailableArithmetic1 = AE(equal.Item1);
                    newAvailableArithmetic2 = AE(equal.Item2);
                    availableBooleanExpressions.Add(equal);
                    availableArithmeticExpressions.UnionWith(newAvailableArithmetic1.Union(newAvailableArithmetic2));
                    break;
                case MicroCTypes.booleanExpression.Great great:
                    newAvailableArithmetic1 = AE(great.Item1);
                    newAvailableArithmetic2 = AE(great.Item2);
                    availableBooleanExpressions.Add(great);
                    availableArithmeticExpressions.UnionWith(newAvailableArithmetic1.Union(newAvailableArithmetic2));
                    break;
                case MicroCTypes.booleanExpression.GreatEqual greatEqual:
                    newAvailableArithmetic1 = AE(greatEqual.Item1);
                    newAvailableArithmetic2 = AE(greatEqual.Item2);
                    availableBooleanExpressions.Add(greatEqual);
                    availableArithmeticExpressions.UnionWith(newAvailableArithmetic1.Union(newAvailableArithmetic2));
                    break;
                case MicroCTypes.booleanExpression.Less less:
                    newAvailableArithmetic1 = AE(less.Item1);
                    newAvailableArithmetic2 = AE(less.Item2);
                    availableBooleanExpressions.Add(less);
                    availableArithmeticExpressions.UnionWith(newAvailableArithmetic1.Union(newAvailableArithmetic2));
                    break;
                case MicroCTypes.booleanExpression.LessEqual lessEqual:
                    newAvailableArithmetic1 = AE(lessEqual.Item1);
                    newAvailableArithmetic2 = AE(lessEqual.Item2);
                    availableBooleanExpressions.Add(lessEqual);
                    availableArithmeticExpressions.UnionWith(newAvailableArithmetic1.Union(newAvailableArithmetic2));
                    break;
                case MicroCTypes.booleanExpression.Not @not:
                    (newAvailableArithmetic1, newAvailableBoolean1) = AE(@not.Item);
                    availableBooleanExpressions.Add(@not);
                    availableBooleanExpressions.UnionWith(newAvailableBoolean1);
                    availableArithmeticExpressions.UnionWith(newAvailableArithmetic1);
                    break;
                case MicroCTypes.booleanExpression.NotEqual notEqual:
                    newAvailableArithmetic1 = AE(notEqual.Item1);
                    newAvailableArithmetic2 = AE(notEqual.Item2);
                    availableBooleanExpressions.Add(notEqual);
                    availableArithmeticExpressions.UnionWith(newAvailableArithmetic1.Union(newAvailableArithmetic2));
                    break;
                case MicroCTypes.booleanExpression.Or @or:
                    (newAvailableArithmetic1, newAvailableBoolean1) = AE(@or.Item1);
                    (newAvailableArithmetic2, newAvailableBoolean2) = AE(@or.Item2);
                    availableBooleanExpressions.Add(@or);
                    availableBooleanExpressions.UnionWith(newAvailableBoolean1.Union(newAvailableBoolean2));
                    availableArithmeticExpressions.UnionWith(newAvailableArithmetic1.Union(newAvailableArithmetic2));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(booleanExpression));
            }

            return (availableArithmeticExpressions, availableBooleanExpressions);
        }

        private HashSet<MicroCTypes.arithmeticExpression> AE(MicroCTypes.arithmeticExpression arithmeticExpression)
        {
            var availableExpressions = new HashSet<MicroCTypes.arithmeticExpression>();
            switch (arithmeticExpression)
            {
                case MicroCTypes.arithmeticExpression.ArrayMember arrayMember:
                    availableExpressions.Add(arrayMember);
                    availableExpressions.UnionWith(AE(arrayMember.Item2));
                    break;
                case MicroCTypes.arithmeticExpression.Divide divide:
                    availableExpressions.UnionWith(AE(divide.Item1));
                    availableExpressions.UnionWith(AE(divide.Item2));
                    break;
                case MicroCTypes.arithmeticExpression.Minus minus:
                    availableExpressions.UnionWith(AE(minus.Item1));
                    availableExpressions.UnionWith(AE(minus.Item2));
                    break;
                case MicroCTypes.arithmeticExpression.Modulo modulo:
                    availableExpressions.UnionWith(AE(modulo.Item1));
                    availableExpressions.UnionWith(AE(modulo.Item2));
                    break;
                case MicroCTypes.arithmeticExpression.Multiply multiply:
                    availableExpressions.UnionWith(AE(multiply.Item1));
                    availableExpressions.UnionWith(AE(multiply.Item2));
                    break;
                case MicroCTypes.arithmeticExpression.Number number:
                    break;
                case MicroCTypes.arithmeticExpression.Plus plus:
                    availableExpressions.UnionWith(AE(plus.Item1));
                    availableExpressions.UnionWith(AE(plus.Item2));
                    break;
                case MicroCTypes.arithmeticExpression.Power power:
                    availableExpressions.UnionWith(AE(power.Item1));
                    availableExpressions.UnionWith(AE(power.Item2));
                    break;
                case MicroCTypes.arithmeticExpression.RecordMember recordMember:
                    break;
                case MicroCTypes.arithmeticExpression.Variable variable:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(arithmeticExpression));
            }

            return availableExpressions;
        }

        private void KillAssignment(AvailableExpressionConstraints availableExpressionConstraints, string variableName)
        {
            availableExpressionConstraints.AvailableArithmeticExpressions
                .ExceptWith(
                    availableExpressionConstraints.AvailableArithmeticExpressions
                        .Where(ae => _freeVariables.FreeVariables(ae).Contains(variableName))
                );
            availableExpressionConstraints.AvailableBooleanExpressions
                .ExceptWith(
                    availableExpressionConstraints.AvailableBooleanExpressions
                        .Where(ae => _freeVariables.FreeVariables(ae).Contains(variableName))
                );
        }

        private void GenerateAssignment(AvailableExpressionConstraints availableExpressionConstraints,
            HashSet<MicroCTypes.arithmeticExpression> expressionAvailableExpressions, string variableName)
        {
            availableExpressionConstraints.AvailableArithmeticExpressions.UnionWith(
                expressionAvailableExpressions
                    .Where(ae => !_freeVariables.FreeVariables(ae).Contains(variableName))
            );
        }
    }
}