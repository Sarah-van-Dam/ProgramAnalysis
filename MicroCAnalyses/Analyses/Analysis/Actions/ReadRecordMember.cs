namespace Analyses.Analysis.Actions
{
    public class ReadRecordMember : Action
    {
        public string RecordName { get; set; }
        public RecordMember RecordMember { get; set; }

        public override string ToSyntax()
            => $"read {RecordName}.{RecordMember};";
    }
}