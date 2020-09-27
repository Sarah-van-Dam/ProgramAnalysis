using System;
using System.Collections.Generic;
using Analyses.Graph;
using Action = Analyses.Analysis.Actions;

namespace Analyses.Analysis.BitVector
{
    public abstract class BitVectorFramework : Analysis
    {
        protected Operator joinOperator;
        protected Direction direction;
        protected Dictionary<Node, IConstraints> finalConstraintsForNodes;

        protected abstract void kill(Edge edge);

        protected abstract void generate(Edge edge);

        public override void Analyse()
        {
            SolveConstraints();
        }

        private void SolveConstraints()
        {
            // Implement worklist algorithm here!
            // For each edges, evaluate all nodes and update the finalConstraintsForNodes with the changes from the edge.
            throw new NotImplementedException();
        }

    }
}