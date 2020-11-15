using Analyses.Algorithms;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Analyses.Analysis.Distributive
{
    public class DangerousVariables : DistributiveFramework
    {
        private readonly ReachingDefinitions _reachingDefinitionsAnalysis;

        public DangerousVariables(ProgramGraph programGraph, WorklistImplementation worklistImplementation = WorklistImplementation.SortedIteration) : base(programGraph, Direction.Backwards, worklistImplementation)
        {
            //TODO: JoinOperator = Operator.Union;
            //TODO: _reachingDefinitionsAnalysis = new FreeVariablesAnalysis();
        }

        public override void InitializeConstraints()
        {
            throw new NotImplementedException();
        }

        protected override void AnalysisFunction(Edge edge, IConstraints constraints)
        {
            throw new NotImplementedException();
        }
    }
}
