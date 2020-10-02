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
            this.ConstructConstraints();
            //this.SolveConstraints();
        }

        /// <summary>
        /// //todo: generate constraints : go through program tree and based on edges do that
        /// </summary>
        private void ConstructConstraints()
        {

            Node nextNode = null;
            //for now - do constraints as strings; afterwards -> turn them into sets
            Dictionary<string, string> constraints = new Dictionary<string, string>();//<nodeName, nodeConstraints>

            //Locate q_start
            foreach (Edge edge in this._program.Edges)
            {
                if (edge.FromNode.Name == "q_start")
                {
                    //hardcode constraint {(x, ?, q_start), (y, ?, q_start)} if Var = {x,y} and Arr = {}
                    //var startNodeConstraint = this.Constraints.VariableToPossibleAssignments["x"];
                    //var startNodeConstraint2 = this.Constraints.VariableToPossibleAssignments["y"];
                    //constraints.Add("q_start", $"{startNodeConstraint}, {startNodeConstraint2}");
                    //store next node
                    nextNode = edge.ToNode;
                }
            }
            //for every node after  q_start
            while (true)
            {
                foreach (Edge edge in this._program.Edges)
                {
                    if (edge.FromNode.Name == nextNode.Name)
                    {
                        string previousNodeConstraint = string.Empty;
                        constraints.TryGetValue(edge.FromNode.Name, out previousNodeConstraint);

                        //constraints.Add(edge.ToNode.Name, $"Is subset of " +
                        //    $"{previousNodeConstraint} minus" +
                        //    $"{Kill(edge)} plus" + //TODO: change these to return values
                        //    $"{Generate(edge)}");
                        //constraintRD(edge.ToNode).IsSubsetOf(constraintRD(edge.fromNode) - killRD(edge) + genRD(edge)));
                        nextNode = edge.ToNode;
                    }
                }
            }
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
            // Implement worklist algorithm here!
            // For each edges, evaluate all nodes and update the finalConstraintsForNodes with the changes from the edge.
            throw new NotImplementedException();

            var programGraph = this._program; //DEBUG 
            var previousRd = "";

            foreach (Edge currentEdge in programGraph.Edges)
            {
                Console.WriteLine(
                    $"Traversing edge {currentEdge.Action} " +
                    $"from node {currentEdge.FromNode.Name} " +
                    $"to node {currentEdge.ToNode.Name}");

                //this.Kill(currentEdge);
                //this.Generate(currentEdge);


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
                    //this.Kill(traversedEdge);
                    //this.Generate(traversedEdge);

                    //var constraintsForNode = Constraints.VariableToPossibleAssignments[selectedNode.Name];
                    //iterationResult.Add(selectedNode, constraintsForNode);


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