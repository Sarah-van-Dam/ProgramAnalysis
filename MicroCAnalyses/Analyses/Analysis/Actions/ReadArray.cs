using Analyses.Helpers;

namespace Analyses.Analysis.Actions
{
    public class ReadArray : Action
    {
        public string ArrayName { get; set; }
        public MicroCTypes.arithmeticExpression Index { get; set; }

        public override string ToSyntax()
            => $"read {ArrayName}[{AstExtensions.AstToString(Index)}];";

        public override string ToString()
        {
            return $"{this.GetType().Name} with ArrayName: {ArrayName}, Index: {Index}";
        }

    }
}