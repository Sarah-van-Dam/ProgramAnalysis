using Analyses.Helpers;

namespace Analyses.Analysis.Actions
{
    public class Write : Action
    {
        public MicroCTypes.arithmeticExpression Expression { get; set; }

        public override string ToSyntax()
            => $"write {AstExtensions.AstToString(Expression)};";

        public override string ToString()
        {
            return $"{this.GetType().Name} with VariableName: {Expression}";
        }

    }
}