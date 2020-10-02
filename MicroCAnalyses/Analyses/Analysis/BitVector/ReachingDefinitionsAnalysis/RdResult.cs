using System.Collections.Generic;

namespace Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis
{
    public class RdResult: AnalysisResult<(string variable, string startNode, string endNode)>
    {
        public RdResult() : base()
        {
        }

        public RdResult(RdResult currentConstraintSet): base(currentConstraintSet)
        {
        }
    }
}