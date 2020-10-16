using Analyses.Analysis.Actions;

namespace Analyses.Helpers
{
    public static class AstExtensions
    {
        
        /// <summary>
        /// Converts an expression from the AST into a string. Parantheses added for clarity of the ordering.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static string AstToString(MicroCTypes.arithmeticExpression expr)
        {
            return expr switch
            {
                MicroCTypes.arithmeticExpression.Divide opr => $"({AstToString(opr.Item1)} / {AstToString(opr.Item2)})",
                MicroCTypes.arithmeticExpression.Minus opr => $"({AstToString(opr.Item1)} - {AstToString(opr.Item2)})",
                MicroCTypes.arithmeticExpression.Modulo opr => $"({AstToString(opr.Item1)} % {AstToString(opr.Item2)})",
                MicroCTypes.arithmeticExpression.Multiply opr =>
                    $"({AstToString(opr.Item1)} * {AstToString(opr.Item2)})",
                MicroCTypes.arithmeticExpression.Plus opr => $"({AstToString(opr.Item1)} + {AstToString(opr.Item2)})",
                MicroCTypes.arithmeticExpression.Power opr => $"({AstToString(opr.Item1)} ^ {AstToString(opr.Item2)})",
                MicroCTypes.arithmeticExpression.RecordMember opr =>
                    $"{opr.Item1}.{(opr.Item2 == 1 ? RecordMember.Fst : RecordMember.Snd)}",
                MicroCTypes.arithmeticExpression.Variable opr => $"{opr.Item}",
                MicroCTypes.arithmeticExpression.Number n => n.Item.ToString(),
                _ => string.Empty
            };
        }

        /// <summary>
        /// Converts a boolean expression from the AST into a string. Parantheses added for clarity of the ordering.
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static string AstToString(MicroCTypes.booleanExpression expr)
        {
            switch (expr)
            {
                case MicroCTypes.booleanExpression.And opr:
                    return $"{AstToString(opr.Item1)} & {AstToString(opr.Item2)}";
                case MicroCTypes.booleanExpression.Equal opr:
                    return $"{AstToString(opr.Item1)} == {AstToString(opr.Item2)}";
                case MicroCTypes.booleanExpression.GreatEqual opr:
                    return $"{AstToString(opr.Item1)} >= {AstToString(opr.Item2)}";
                case MicroCTypes.booleanExpression.Great opr:
                    return $"{AstToString(opr.Item1)} > {AstToString(opr.Item2)}";
                case MicroCTypes.booleanExpression.LessEqual opr:
                    return $"{AstToString(opr.Item1)} <= {AstToString(opr.Item2)}";
                case MicroCTypes.booleanExpression.Less opr:
                    return $"{AstToString(opr.Item1)} < {AstToString(opr.Item2)}";
                case MicroCTypes.booleanExpression.NotEqual opr:
                    return $"{AstToString(opr.Item1)} != {AstToString(opr.Item2)}";
                case MicroCTypes.booleanExpression.Not opr:
                    return $"!{AstToString(opr.Item)}";
                case MicroCTypes.booleanExpression.Or opr:
                    return $"{AstToString(opr.Item1)} | {AstToString(opr.Item2)}";
                default:
                    if (expr.Equals(MicroCTypes.booleanExpression.False)) return "false";
                    else if (expr.Equals(MicroCTypes.booleanExpression.True)) return "true";
                    else return string.Empty;
            }
        }
    }
}