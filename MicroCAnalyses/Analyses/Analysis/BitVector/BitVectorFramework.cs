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

        public override AnalysisResult Analyse()
        {
            return this.SolveConstraints();
        }

        /// <summary>
        /// Worklist algorithm;
        /// For each edge, evaluate all nodes and update the finalConstraintsForNodes 
        /// with the changes from the edge.
        /// </summary>
        private AnalysisResult SolveConstraints()
        {
            var programGraph = this._program; //DEBUG 

            bool extraRoundNeeded = true;
            int step = 0;
            AnalysisResult result = new AnalysisResult();
            Edge traversedEdge = null;
            //Node selectedNode = programGraph.Nodes.First(); - not supported by hashsets
            Node selectedNode = null;

            Dictionary<Node, IConstraints> iterationResult = new Dictionary<Node, IConstraints>();

            while (extraRoundNeeded)
            {               
                foreach (Edge outgoingEdge in programGraph.Edges)
                {
                    traversedEdge = outgoingEdge;
                    selectedNode = traversedEdge.ToNode;
                    this.Kill(traversedEdge);
                    this.Generate(traversedEdge);                    
                    iterationResult.Add(selectedNode, Constraints.Where(x => x.Node == selectedNode))
                        
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