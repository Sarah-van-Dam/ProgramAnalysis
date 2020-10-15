namespace Analyses.Analysis.Actions
{
    public class RecordAssignment : Action
    {
        public string RecordName { get; set; }
        public MicroCTypes.arithmeticExpression FirstExpression { get; set; }
        public MicroCTypes.arithmeticExpression SecondExpression { get; set; }

        public override string ToSyntax()
            => $"{RecordName} := ({FirstExpression}, {SecondExpression});";

        public override string ToString()
        {
            return $"{this.GetType().Name} with RecordName: {RecordName}, FirstExpression: {FirstExpression} and SecondExpression: {SecondExpression}";
        }

    }
}