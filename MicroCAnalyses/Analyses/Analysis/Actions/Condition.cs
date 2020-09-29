namespace Analyses.Analysis.Actions
{
    public class Condition : Action
    {
        public string Cond { get; set; }

        public override string ToString()
            => Cond;
    }
}
