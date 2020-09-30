using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Analyses.Graph;
using Action = Analyses.Analysis.Actions;

[assembly: InternalsVisibleTo("Analyses.Test")]
namespace Analyses.Analysis.BitVector
{
    public abstract class BitVectorFramework : Analysis
    {
        protected Operator JoinOperator;
        protected Direction Direction;
        protected Dictionary<Node, IConstraints> FinalConstraintsForNodes;
        internal Dictionary<Node, IConstraints> Constraints
        {
            get => FinalConstraintsForNodes;
        }

        protected BitVectorFramework(ProgramGraph programGraph)
        {
            _program = programGraph;
            FinalConstraintsForNodes = new Dictionary<Node, IConstraints>();
        }
        public abstract void Kill(Edge edge, IConstraints constraints);

        public abstract void Generate(Edge edge, IConstraints constraints);

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