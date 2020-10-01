namespace Analyses.Analysis.Actions
{
    public class Write : Action
    {
        public string VariableName { get; set; }

        public override string ToSyntax()
            => $"write {VariableName};";
    }
}