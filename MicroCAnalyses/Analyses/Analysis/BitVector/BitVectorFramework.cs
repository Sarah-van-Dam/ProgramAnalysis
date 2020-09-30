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
            return this.SolveConstraints();
        }

        /// <summary>
        /// Worklist algorithm;
        /// For each edge, evaluate all nodes and update the finalConstraintsForNodes 
        /// with the changes from the edge.
        /// </summary>
        private void SolveConstraints()
        {
            var programGraph = this._program; //DEBUG 

            bool extraRoundNeeded = true;
            int step = 0;
            Dictionary<Node, Constraints> result = new Dictionary<Node, Constraints>();
            Edge traversedEdge = null;
            //Node selectedNode = programGraph.Nodes.First(); - not supported by hashsets
            Node selectedNode = null;

            Dictionary<Node, Constraints> iterationResult = new Dictionary<Node, Constraints>();

            while (extraRoundNeeded)
            {               
                foreach (Edge outgoingEdge in programGraph.Edges)
                {

                    traversedEdge = outgoingEdge;
                    selectedNode = traversedEdge.ToNode;
                    this.Kill(traversedEdge);
                    this.Generate(traversedEdge);
                    var constraints = Constraints.VariableToPossibleAssignments[traversedEdge.Action.ToString()];
                    foreach (HashSet<(string action, string startNode, string endNode)> constraint in constraints)
                    {
                        //build constraint 


                        iterationResult.Add(selectedNode, Constraints.VariableToPossibleAssignments[traversedEdge.Action.ToString()]);
                    }

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