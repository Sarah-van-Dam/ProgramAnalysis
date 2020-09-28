using System;
using System.Collections.Generic;
using Analyses.Graph;
using Action = Analyses.Analysis.Actions;

namespace Analyses.Analysis.BitVector
{
    public abstract class BitVectorFramework : Analysis
    {
        protected Operator JoinOperator;
        protected Direction Direction;
        protected Dictionary<Node, IConstraints> FinalConstraintsForNodes;

        public abstract void Kill(Edge edge);

        public abstract void Generate(Edge edge);

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