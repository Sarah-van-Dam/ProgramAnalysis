namespace Analyses.Analysis.Actions
{
    public class RecordMemberAssignment : Action
    {
        public string RecordName { get; set; }
        public RecordMember RecordMember { get; set; }
        public string RightHandSide { get; set; }

        public override string ToSyntax()
            => $"{RecordName}.{RecordMember} := {RightHandSide};";

        public override string ToString()
        {
            return $"{this.GetType().Name} with RecordName: {RecordName}, RecordMember: {RecordMember} and RightHandSide: {RightHandSide}";
        }

    }
}