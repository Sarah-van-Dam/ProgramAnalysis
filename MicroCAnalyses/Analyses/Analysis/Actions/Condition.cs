namespace Analyses.Analysis.Actions
{
    public class Condition : Action
    {
        public string Cond { get; set; }

        public override string ToSyntax()
            => Cond;

        public override string ToString()
        {
            return $"{this.GetType().Name} with Cond: {Cond}";
        }

    }
}
