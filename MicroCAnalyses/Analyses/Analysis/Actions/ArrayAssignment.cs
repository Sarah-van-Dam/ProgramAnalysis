namespace Analyses.Analysis.Actions
{
    public class ArrayAssignment : Action
    {
        public string ArrayName { get; set; }
        public string Index { get; set; }
        public string RightHandSide { get; set; }

        public override string ToString()
        {
            return $"{this.GetType().Name} with ArrayName: {ArrayName}, Index: {Index} and RightHandSide: {RightHandSide}";
        }
    }
}