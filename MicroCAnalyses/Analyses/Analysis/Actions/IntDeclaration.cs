
namespace Analyses.Analysis.Actions
{
    public class IntDeclaration : Action
    {
        public string VariableName { get; set; }

        public override string ToSyntax()
            => $"int {VariableName};";
    }
}