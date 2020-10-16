namespace Analyses.Analysis.Actions
{
    public class Condition : Action
    {
        public MicroCTypes.booleanExpression Cond { get; set; }

        public string GraphvizSyntax { get; set; }

        public override string ToSyntax()
            => GraphvizSyntax;

        public override string ToString()
        {
            return $"{this.GetType().Name} with Cond: {GraphvizSyntax}";
        }

    }
}
