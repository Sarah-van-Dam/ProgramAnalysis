using System;
using System.Collections.Generic;
using Analyses.Analysis.Actions;

namespace Analyses.Analysis
{
    public class FreeVariablesAnalysis
    {
        public HashSet<string> FreeVariables(MicroCTypes.booleanExpression booleanExpression)
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

        public HashSet<string> FreeVariables(MicroCTypes.arithmeticExpression arithmeticExpression)
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
                    // A number isn't a free variable -> ?? mta
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
    }
}