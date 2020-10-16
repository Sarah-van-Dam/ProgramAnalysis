namespace Analyses.Analysis.Actions
{
    public class Write : Action
    {
        public MicroCTypes.arithmeticExpression Expression { get; set; }

        public override string ToSyntax()
            => $"write {Expression};";

        public override string ToString()
        {
            return $"{this.GetType().Name} with VariableName: {Expression}";
        }

    }
}