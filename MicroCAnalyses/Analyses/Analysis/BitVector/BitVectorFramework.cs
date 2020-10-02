using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Analyses;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
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
        protected Dictionary<Node, RdResult> AnalysisResult = new Dictionary<Node, RdResult>();

        protected BitVectorFramework(ProgramGraph programGraph)
        {
            _program = programGraph;
            FinalConstraintsForNodes = new Dictionary<Node, IConstraints>();
        }
        public abstract void Kill(Edge edge, IConstraints constraints);

        public abstract void Generate(Edge edge, IConstraints constraints);

        public override void Analyse()
        {
            var graph = this._program.ExportToGV();
            this.ConstructConstraints();
            //this.SolveConstraints();
        }

        /// <summary>
        /// todo: generate constraints : go through program tree and based on edges do that
        /// </summary>
        private void ConstructConstraints()
        {
            //Locate q_start
            var startNodeConstraint = 
                this.Constraints
                    .FirstOrDefault(x => x.Key.Name == "q_start");
            startNodeConstraint.DebugPrint();

            //construct constraint set for q_start
            var resultConstraints = this.ConstructConstraintForNode(startNodeConstraint);
            AnalysisResult.Add(startNodeConstraint.Key, resultConstraints);

            //for every node after the 1st one
            foreach (var node in this.Constraints.Skip(1))
            {
                //for every edge going out of that node 
                foreach (Edge edge in this._program.Edges
                    .Where( x => x.FromNode.Name == node.Key.Name))
                {
                    Console.WriteLine($"Traversing edge {edge}");
                    this.Kill(edge, node.Value);
                    this.Generate(edge, node.Value);
                    node.DebugPrint();

                    foreach (var hashSet in (node.Value as ReachingDefinitionConstraints)
                        .VariableToPossibleAssignments.Values)
                    {
                        resultConstraints.ExceptWith(hashSet);
                    }
                    Console.WriteLine(resultConstraints.AllToString());
                    AnalysisResult.Add(node.Key, resultConstraints);
                }
            }
        }

        private RdResult ConstructConstraintForNode(KeyValuePair<Node, IConstraints> startNodeConstraint)
        {
            RdResult result = new RdResult();
            foreach (var hashSet in (startNodeConstraint.Value as ReachingDefinitionConstraints).VariableToPossibleAssignments.Values)
            {
                foreach (var triple in hashSet)
                {
                    result.Add(triple);
                }
            }
            //debug print using result object
            Console.WriteLine(result.AllToString());
            return result;
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

public class RdResult: HashSet<(string variable, string startNode, string endNode)>
{

}

public static class HashSetExtensions
{
    public static string AllToString(this HashSet<(string, string, string)> hashset)
    {
        lock (hashset)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in hashset)
                sb.Append(item);
            return sb.ToString();
        }
    }

    public static void DebugPrint(this KeyValuePair<Node, IConstraints>  startNodeConstraint)
    {
        var rdConstraints = (startNodeConstraint.Value as ReachingDefinitionConstraints).VariableToPossibleAssignments;

        //debug print using string
        var str = $"RD({startNodeConstraint.Key.Name}) is a superset of " +
                  "{";
        foreach (var line in rdConstraints)
        {
            str = str + $"{line.Value.AllToString()},";
        }
        str = str.Remove(str.Length - 1) + "}";
        Console.WriteLine(str);
    }
}

