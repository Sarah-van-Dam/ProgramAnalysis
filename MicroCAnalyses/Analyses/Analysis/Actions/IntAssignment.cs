namespace Analyses.Analysis.Actions
{
    public class IntAssignment : Action
    {
        public string VariableName { get; set; }
        public MicroCTypes.arithmeticExpression RightHandSide { get; set; }

        public override string ToSyntax()
            => $"{VariableName} := {RightHandSide};";

        public override string ToString()
        {
            return $"{this.GetType().Name} with VariableName: {VariableName}, RightHandSide: {RightHandSide}";
        }

    }
    
    
}