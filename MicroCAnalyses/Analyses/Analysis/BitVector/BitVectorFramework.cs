using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Analyses.Algorithms;
using Analyses.Graph;
using Analyses.Helpers;
using Action = Analyses.Analysis.Actions;

[assembly: InternalsVisibleTo("Analyses.Test")]

namespace Analyses.Analysis.BitVector
{
    public abstract class BitVectorFramework : Analysis
    {
        protected Operator JoinOperator;
        public readonly Dictionary<Node, IConstraints> FinalConstraintsForNodes;

        protected BitVectorFramework(ProgramGraph programGraph, Direction direction,
            WorklistImplementation worklistImplementation) : base(programGraph, direction, worklistImplementation)
        {
            FinalConstraintsForNodes = new Dictionary<Node, IConstraints>();
        }

        public abstract void Kill(Edge edge, IConstraints constraints);

        public abstract void Generate(Edge edge, IConstraints constraints);

        /// <summary>
        /// Locates q_start and construct its constraints;
        /// Afterwards for each node traverses every edge and constructs additional constraints
        /// </summary>
        public override void Analyse()
        {
            InitializeConstraints();
            foreach (var node in _program.Nodes)
            {
                _worklistAlgorithm.Insert(node);
            }

            var isForward = Direction == Direction.Forward;
            while (!_worklistAlgorithm.Empty())
            {
                var node = _worklistAlgorithm.Extract();
                foreach (var edge in isForward ? node.OutGoingEdges : node.InGoingEdges)
                {
                    var updated = UpdateConstraints(edge, isForward);
                    if (updated)
                    {
                        _worklistAlgorithm.Insert(isForward ? edge.ToNode : edge.FromNode);
                    }
                }
            }
        }

        private KeyValuePair<Node, IConstraints> GetConstraintsOfNode(string toNodeName)
        {
            return FinalConstraintsForNodes
                .Single(x => x.Key.Name == toNodeName);
        }

        public abstract void InitializeConstraints();

        private bool UpdateConstraints(Edge edge, bool isForward)
        {
            var edgeStartConstraints = GetConstraintsOfNode(edge.FromNode.Name).Value;
            var edgeEndConstraints = GetConstraintsOfNode((edge.ToNode.Name)).Value;
            bool wasUpdated;
            if (isForward)
            {
                wasUpdated = HandleUpdateOfConstraints(edgeStartConstraints, edgeEndConstraints, edge);
            }
            else
            {
                wasUpdated = HandleUpdateOfConstraints(edgeEndConstraints, edgeStartConstraints, edge);
            }

            return wasUpdated;
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
                    UpdateConstraintsForNode(isForward ? edge.ToNode : edge.FromNode, inMemConstraint);
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
            Kill(edge, inMemConstraint);
            Generate(edge, inMemConstraint);
            return inMemConstraint;
        }
    }
}