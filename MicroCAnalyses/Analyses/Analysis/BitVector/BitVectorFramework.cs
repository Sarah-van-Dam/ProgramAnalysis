using System.Collections.Generic;
using System.Linq;
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
        public readonly Dictionary<Node, IConstraints> FinalConstraintsForNodes;

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

        private KeyValuePair<Node, IConstraints> GetConstraintsOfNode(string toNodeName)
        {
            return FinalConstraintsForNodes
                    .Single(x => x.Key.Name == toNodeName);
        }

        public abstract void InitializeConstraints();
        
        
        /// <summary>
        /// Locates q_start and construct its constraints;
        /// Afterwards for each node traverses every edge and constructs additional constraints
        /// </summary>
        private void SolveConstraints()
        {
            InitializeConstraints();
            var isForward = Direction == Direction.Forward;

            var orderedEdgesList = OrderEdgesByDirection(isForward);
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

        private void UpdateConstraints(Edge edge, bool isForward, ref int edgesThatWasUpdated)
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

            if (wasUpdated)
            {
                edgesThatWasUpdated++;
            }
        }

        private List<Edge> OrderEdgesByDirection(bool isForward)
        {
            var edgesList = new List<Edge>();
            var nodeList = _program.Nodes.ToList();
            nodeList.Sort((first, second) =>
            {
                if (first.Name == ProgramGraph.EndNode)
                {
                    return int.MaxValue;
                }

                return first.Index - second.Index;
            });
            foreach (var node in nodeList)
            {
                var edges = _program.Edges.Where(e => e.FromNode.Equals(node)).ToList();
                edges.Sort((first, second) =>
                {
                    if (second.ToNode.Name == ProgramGraph.EndNode)
                    {
                        return int.MaxValue;
                    }

                    return first.ToNode.Index - second.ToNode.Index;
                });
                edgesList.AddRange(edges);
            }

            if (!isForward)
            {
                edgesList.Reverse();
            }

            return edgesList;
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
            Kill(edge, inMemConstraint);
            Generate(edge, inMemConstraint);
            return inMemConstraint;
        }
    }
}