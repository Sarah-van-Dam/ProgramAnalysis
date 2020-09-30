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

            foreach (Edge outgoingEdge in programGraph.Edges)
            {
                Console.WriteLine(
                    $"Traversing edge {outgoingEdge.Action} " +
                    $"from node {outgoingEdge.FromNode.Name} " +
                    $"to node {outgoingEdge.ToNode.Name}");


            }
        }

        /// <summary>
        /// Worklist algorithm;
        /// For each edge, evaluate all nodes and update the finalConstraintsForNodes 
        /// with the changes from the edge.
        /// </summary>
        private void SolveConstraints1()
        {
            var programGraph = this._program; //DEBUG 

            bool extraRoundNeeded = true;
            int step = 0;
            Dictionary<Node, Constraints> result = new Dictionary<Node, Constraints>();
            Edge traversedEdge = null;
            //Node selectedNode = programGraph.Nodes.Single(n => n.Name == "q_start"); - doenst work
            Node selectedNode = null;

            Dictionary<Node, Constraints> iterationResult = new Dictionary<Node, Constraints>();


            while (extraRoundNeeded)
            {               
                foreach (Edge outgoingEdge in programGraph.Edges)
                {
                    Console.WriteLine($"Traversing edge {outgoingEdge.Action} from node {outgoingEdge.FromNode.Name} to node {outgoingEdge.ToNode.Name}");
                    traversedEdge = outgoingEdge;
                    selectedNode = traversedEdge.ToNode;
                    this.Kill(traversedEdge);
                    this.Generate(traversedEdge);


                    //var constraints = Constraints.VariableToPossibleAssignments[traversedEdge.Action.ToString()];
                    //var resultConstraints = this.ApplyOperator(constraints);


                    //foreach ((string action, string startNode, string endNode) constraint in constraints)
                    //{
                        //build constraint 



                        //add it to iteration result
                        //iterationResult.Add(selectedNode, );
                    //}

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
}