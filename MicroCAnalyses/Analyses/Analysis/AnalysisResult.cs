using System;
using System.Collections.Generic;
using System.Text;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;

namespace Analyses.Analysis
{
    /// <summary>
    /// Wrapper for a RdResult, which is wrapper for a HashSet<string, string, string>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AnalysisResult<T> : HashSet<T>
    {
        public AnalysisResult()
        {
        }

        public AnalysisResult(AnalysisResult<T> currentConstraintSet) : base(currentConstraintSet)
        {
        }

        public string AllToString()
        {
            lock (this)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in this)
                    sb.Append(item);
                return sb.ToString();
            }
        }
    }
}