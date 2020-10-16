using System.Collections.Generic;
using Analyses.Graph;
using Analyses.Helpers;

namespace Analyses.Analysis.Distributive
{
    public abstract class DistributiveFramework : Analysis
    {
        protected Operator JoinOperator;
        protected Direction Direction;
        public readonly Dictionary<Node, IConstraints> FinalConstraintsForNodes;

        protected DistributiveFramework(ProgramGraph programGraph)
        {
            _program = programGraph;
            FinalConstraintsForNodes = new Dictionary<Node, IConstraints>();
        }
        
        public abstract void InitializeConstraints();
        protected abstract void AnalysisFunction(Edge edge, IConstraints constraints);
     
        public override void Analyse()
        {
            InitializeConstraints();
            var isForward = Direction == Direction.Forward;

            var orderedEdgesList = EdgesHelper.OrderEdgesByDirection(_program, isForward);
            var edgesThatWasUpdated = 0;
            
            foreach (var edge in orderedEdgesList)
            {
                UpdateConstraints(edge, isForward, ref edgesThatWasUpdated);
            }

            while (edgesThatWasUpdated != 0)
            {
                edgesThatWasUpdated = 0;
                foreach (var edge in orderedEdgesList)
                {
                    UpdateConstraints(edge, isForward, ref edgesThatWasUpdated);
                }
            }
        }

        private void UpdateConstraints(Edge edge, in bool isForward, ref int edgesThatWasUpdated)
        {
            var edgeStartConstraints = FinalConstraintsForNodes[edge.FromNode];
            var edgeEndConstraints = FinalConstraintsForNodes[edge.ToNode];
            bool wasUpdated;
            if (isForward)
            {
                wasUpdated = HandleUpdateOfConstraints(edgeStartConstraints, edgeEndConstraints, edge);
            }
            else
            {
                wasUpdated = HandleUpdateOfConstraints(edgeEndConstraints, edgeStartConstraints, edge);
            }

            if (wasUpdated)
            {
                edgesThatWasUpdated++;
            }
        }
        
        private bool HandleUpdateOfConstraints(IConstraints leftHandSide, IConstraints rightHandSide, Edge edge)
        {
            var updated = false;
            var isForward = Direction == Direction.Forward;
            var inMemConstraint = GenerateInMemoryConstraints(leftHandSide, edge);
            if (JoinOperator == Operator.Union)
            {
                if (!(inMemConstraint.IsSubset(rightHandSide)))
                {
                    UpdateConstraintsForNode(isForward ? edge.ToNode : edge.FromNode, inMemConstraint);
                    updated = true;
                }
            }
            else
            {
                if (!(inMemConstraint.IsSuperSet(rightHandSide)))
                {
                    UpdateConstraintsForNode(isForward ? edge.ToNode: edge.FromNode, inMemConstraint);
                    updated = true;
                }
            }

            return updated;
        }
        
        private void UpdateConstraintsForNode(Node node, IConstraints inMemConstraint)
        {
            var constraints = FinalConstraintsForNodes[node];
            constraints.Join(inMemConstraint);
        }

        /// <summary>
        /// Clones constraints and applies edge kill and gen sets to it
        /// </summary>
        /// <param name="edgeStartConstraints"></param>
        /// <param name="edge"></param>
        /// <returns></returns>
        private IConstraints GenerateInMemoryConstraints(IConstraints edgeStartConstraints, Edge edge)
        {
            var inMemConstraint = edgeStartConstraints.Clone();
            AnalysisFunction(edge, inMemConstraint);
            return inMemConstraint;
        }
    }
}