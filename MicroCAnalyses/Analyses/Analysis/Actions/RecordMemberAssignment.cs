namespace Analyses.Analysis.Actions
{
    public class RecordMemberAssignment : Action
    {
        public string RecordName { get; set; }
        public RecordMember RecordMember { get; set; }
        public string RightHandSide { get; set; }

        public override string ToString()
            => $"{RecordName}.{RecordMember} := {RightHandSide};";
    }
}