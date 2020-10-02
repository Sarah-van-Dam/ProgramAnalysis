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
            this.ConstructConstraints();
            this.AnalysisResult.DebugPrint();
        }

        /// <summary>
        /// todo: generate constraints : go through program tree and based on edges do that
        /// </summary>
        private void ConstructConstraints()
        {
            //Locate q_start
            String nodeName = "q_start";
            var startNodeConstraint = 
                this.Constraints
                    .FirstOrDefault(x => x.Key.Name == nodeName);
            startNodeConstraint.DebugPrint();

            //construct constraint set for q_start
            var resultConstraints = this.ConstructConstraintForStartNode(startNodeConstraint);
            AnalysisResult.Add(startNodeConstraint.Key, resultConstraints);

            //for every edge going out of that node 
            foreach (Edge edge in this._program.Edges
                .Where( x => x.FromNode.Name == nodeName))
            {
                Console.WriteLine($"Traversing edge {edge}");
                nodeName = edge.ToNode.Name;
                var node = 
                    this.Constraints
                        .FirstOrDefault(x => x.Key.Name == nodeName);
                
                //the killRD
                this.Kill(edge, node.Value);
                foreach (var hashSet in (node.Value as ReachingDefinitionConstraints)
                    .VariableToPossibleAssignments)
                {
                    //Console.WriteLine(hashSet.AllToString());
                    resultConstraints
                        .RemoveWhere(tuple => tuple.variable.Contains(hashSet.Key));
                }
                Console.WriteLine($"After killRD: {resultConstraints.AllToString()}");


                //the genRD
                this.Generate(edge, node.Value);
                foreach (var hashSet in (node.Value as ReachingDefinitionConstraints)
                    .VariableToPossibleAssignments.Values)
                {
                    Console.WriteLine(hashSet.AllToString());
                    resultConstraints.UnionWith(hashSet);
                }
                Console.WriteLine($"After genRD: {resultConstraints.AllToString()}");

                AnalysisResult.Add(node.Key, resultConstraints);
                //AnalysisResult.DebugPrint();
            }
            
        }

        private RdResult ConstructConstraintForStartNode(KeyValuePair<Node, IConstraints> startNodeConstraint)
        {
            RdResult result = new RdResult();
            foreach (var hashSet in (startNodeConstraint.Value as ReachingDefinitionConstraints).VariableToPossibleAssignments.Values)
            {
                foreach (var triple in hashSet)
                {
                    result.Add(triple);
                }
            }
            return result;
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

    public static void DebugPrint(this Dictionary<Node, RdResult> analysisResult)
    {
        foreach (var node in analysisResult)
        {
            Console.WriteLine(node.Key);
            Console.WriteLine(node.Value.AllToString());
        }
    }
}

