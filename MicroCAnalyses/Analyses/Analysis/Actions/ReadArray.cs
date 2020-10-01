namespace Analyses.Analysis.Actions
{
    public class ReadArray : Action
    {
        public string ArrayName { get; set; }
        public string Index { get; set; }

        public override string ToSyntax()
            => $"read {ArrayName}[{Index}];";
    }
}