namespace Analyses.Analysis.Actions
{
    public class RecordDeclaration : Action
    {
        public string VariableName { get; set; }

        public override string ToSyntax()
            => $"{{ int {RecordMember.Fst}; int {RecordMember.Snd} }} {VariableName};";

        public override string ToString()
        {
            return $"{this.GetType().Name} with VariableName: {VariableName}";
        }

    }
}