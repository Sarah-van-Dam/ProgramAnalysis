namespace Analyses.Analysis.Actions
{
    public class RecordAssignment : Action
    {
        public string RecordName { get; set; }
        public string FirstExpression { get; set; }
        public string SecondExpression { get; set; }

        public override string ToString()
        {
            return $"{this.GetType().Name} with RecordName: {RecordName}, FirstExpression: {FirstExpression} and SecondExpression: {SecondExpression}";
        }
    }
}