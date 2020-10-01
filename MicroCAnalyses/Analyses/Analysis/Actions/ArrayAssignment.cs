namespace Analyses.Analysis.Actions
{
    public class ArrayAssignment : Action
    {
        public string ArrayName { get; set; }
        public string Index { get; set; }
        public string RightHandSide { get; set; }

        public override string ToSyntax()
            => $"{ArrayName}[{Index}] := {RightHandSide};";
    }
}