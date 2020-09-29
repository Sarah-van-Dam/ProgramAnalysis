namespace Analyses.Analysis.Actions
{
    public class RecordDeclaration : Action
    {
        public string VariableName { get; set; }

        public override string ToString()
            => $"{{ int {RecordMember.Fst}; int {RecordMember.Snd} }} {VariableName};";
    }
}