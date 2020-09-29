namespace Analyses.Analysis.Actions
{
    public class RecordAssignment : Action
    {
        public string RecordName { get; set; }
        public string FirstExpression { get; set; }
        public string SecondExpression { get; set; }

        public override string ToString()
            => $"{RecordName} := ({FirstExpression}, {SecondExpression});";
    }
}