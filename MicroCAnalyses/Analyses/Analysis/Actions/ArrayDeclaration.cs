namespace Analyses.Analysis.Actions
{
    public class ArrayDeclaration : Action
    {
        public string ArrayName { get; set; }
        public int ArraySize { get; set; }

        public override string ToSyntax()
            => $"int[{ArraySize}] {ArrayName};";

        public override string ToString()
        {
            return $"ArrayDeclation with name: {ArrayName} and size {ArraySize}";
        }

    }
}