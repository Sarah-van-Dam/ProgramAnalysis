namespace Analyses.Analysis.Actions
{
    public class ArrayAssignment : Action
    {
        public string ArrayName { get; set; }
        public MicroCTypes.arithmeticExpression Index { get; set; }
        public MicroCTypes.arithmeticExpression RightHandSide { get; set; }

        public override string ToSyntax()
            => $"{ArrayName}[{Index}] := {RightHandSide};";

        public override string ToString()
        {
            return $"{this.GetType().Name} with ArrayName: {ArrayName}, Index: {Index} and RightHandSide: {RightHandSide}";
        }

    }
}