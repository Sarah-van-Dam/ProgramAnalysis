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
        protected Dictionary<Node, Constraints> FinalConstraintsForNodes;
        internal abstract Constraints Constraints { get; }


        public abstract void Kill(Edge edge);

        public abstract void Generate(Edge edge);

        public override void Analyse()
        {
            this.SolveConstraints();
        }

        private void BuildConstraintForEdge(Edge edge)
        {

        }



        private void ApplyOperator(
            HashSet<(string action, string startNode, string endNode)> constraint)
        {
            //extract left and right constraint
            (string action, string startNode, string endNode) leftConstraint = ("", "", "");
            (string action, string startNode, string endNode) rightConstraint = ("", "", "");


            if (this.JoinOperator == Operator.Union)
            {
                //leftConstraint.UnionWith(rightConstraint);
            }
            else if (this.JoinOperator == Operator.Intersection)
            {
                //leftConstraint.IntersectWith(rightConstraint);
            }
            else
            {
                throw new SystemException("Illegal constraint operator");
            }
            //return leftConstraint;
        }

        private void SolveConstraints()
        {
            var programGraph = this._program; //DEBUG 
            var previousRd = "";

            foreach (Edge currentEdge in programGraph.Edges)
            {
                Console.WriteLine(
                    $"Traversing edge {currentEdge.Action} " +
                    $"from node {currentEdge.FromNode.Name} " +
                    $"to node {currentEdge.ToNode.Name}");




            }
        }

        /// <summary>
        /// Worklist algorithm;
        /// For each edge, evaluate all nodes and update the finalConstraintsForNodes 
        /// with the changes from the edge.
        /// </summary>
        private void SolveConstraintsRoundRobin()
        {
            var programGraph = this._program; //DEBUG 

            bool extraRoundNeeded = true;
            int step = 0;
            Edge traversedEdge = null;
            //Node selectedNode = programGraph.Nodes.First(); - not supported by hashsets
            Node selectedNode = null;

            Dictionary<Node, HashSet<(string, string, string)>> result =
                new Dictionary<Node, HashSet<(string, string, string)>>();
            Dictionary<Node, HashSet<(string, string, string)>> iterationResult = 
                new Dictionary<Node, HashSet<(string, string, string)>>();

            while (extraRoundNeeded)
            {
                foreach (Edge outgoingEdge in programGraph.Edges)
                {
                    traversedEdge = outgoingEdge;
                    selectedNode = traversedEdge.ToNode;
                    this.Kill(traversedEdge);
                    this.Generate(traversedEdge);

                    var constraintsForNode = Constraints.VariableToPossibleAssignments[selectedNode.Name];                
                    iterationResult.Add(selectedNode, constraintsForNode);


                    step++;

                }
                if (iterationResult != result)
                {
                    result = iterationResult;
                }
                else // entire traversal has given rise to no changes
                {
                    extraRoundNeeded = false;
                }
            }

    }
}