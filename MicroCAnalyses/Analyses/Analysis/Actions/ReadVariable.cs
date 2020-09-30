namespace Analyses.Analysis.Actions
{
    public class ReadVariable : Action
    {
        public string VariableName { get; set; }

        public override string ToString()
        {
            return $"{this.GetType().Name} with VariableName: {VariableName}";
        }

    }
}