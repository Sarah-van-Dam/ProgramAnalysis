namespace Analyses.Analysis.Actions
{
    public class ArrayDeclaration : Action
    {
        public string ArrayName { get; set; }
        public int ArraySize { get; set; }

        public override string ToString()
            => $"int[{ArraySize}] {ArrayName};";
    }
}