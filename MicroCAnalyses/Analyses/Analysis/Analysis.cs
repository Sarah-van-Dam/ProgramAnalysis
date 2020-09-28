using Analyses.Graph;

namespace Analyses.Analysis
{
    public abstract class Analysis
    {
        protected ProgramGraph _program;

        
        // ToDo: We need to figure out the return type the analyse method.
        public abstract void Analyse();
        
        
    }
}