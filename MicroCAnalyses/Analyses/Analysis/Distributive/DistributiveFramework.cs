using System;
using System.Collections.Generic;
using Analyses.Algorithms;
using Analyses.Graph;
using Analyses.Helpers;

namespace Analyses.Analysis.Distributive
{
    public abstract class DistributiveFramework : Analysis
    {
        protected Operator JoinOperator;
        public readonly Dictionary<Node, IConstraints> FinalConstraintsForNodes;

        protected DistributiveFramework(ProgramGraph programGraph, Direction direction, WorklistImplementation worklistImplementation) : base(programGraph, direction, worklistImplementation)
        {
            FinalConstraintsForNodes = new Dictionary<Node, IConstraints>();
        }
        
        public abstract void InitializeConstraints();
        protected abstract void AnalysisFunction(Edge edge, IConstraints constraints);
     
        public override void Analyse()
        {
            InitializeConstraints();
            var isForward = Direction == Direction.Forward;
            
            foreach (var node in _program.Nodes)
            {
                _worklistAlgorithm.Insert(node);
            }

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
        
        public override void PrintResult()
        {
            Console.WriteLine($"Analysis completed in {_worklistAlgorithm.BasicActionsNeeded} steps");
            foreach (var (node, constraints) in FinalConstraintsForNodes)
            {
                Console.WriteLine($"{node}: {constraints}");
            }
        }

        private bool UpdateConstraints(Edge edge, in bool isForward)
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